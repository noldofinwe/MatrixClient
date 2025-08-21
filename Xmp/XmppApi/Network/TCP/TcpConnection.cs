using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using Logging;
using XmppApi.Network.Events;

namespace XmppApi.Network.TCP
{
  public class TcpConnection : AbstractConnection, IDisposable
  {
    //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\

    #region --Attributes--

    private const string LOGGER_TAG = "[TcpConnection]: ";

    /// <summary>
    /// How many characters should get read at once max.
    /// </summary>
    private const int BUFFER_SIZE = 4096;

    /// <summary>
    /// The timeout in ms for TCP connections.
    /// </summary>
    private const int CONNECTION_TIMEOUT_MS = 3000;

    /// <summary>
    /// The timeout for upgrading to a TLS connection in ms.
    /// </summary>
    private const int TLS_UPGRADE_TIMEOUT_MS = 5000;

    /// <summary>
    /// The timeout for sending data.
    /// </summary>
    private const int SEND_TIMEOUT_MS = 1000;

    /// <summary>
    /// The maximum amount of attempts to connect until we fail.
    /// </summary>
    private const int MAX_CONNECTION_ATTEMPTS = 3;

    /// <summary>
    /// The time in ms between connection attempts.
    /// </summary>
    private const int CONNECTION_ATTEMPT_DELAY = 1000;

    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private SslStream sslNetworkStream;

    private StreamReader reader;
    private StreamWriter writer;

    /// <summary>
    /// Disables the TCP connection timeout defined in <see cref="CONNECTION_TIMEOUT_MS"/>.
    /// Default = false.
    /// </summary>
    public bool disableConnectionTimeout;

    /// <summary>
    /// Disables the TLS upgrade timeout defined in <see cref="TLS_UPGRADE_TIMEOUT_MS"/>.
    /// Default = false.
    /// </summary>
    public bool disableTlsUpgradeTimeout;

    private ConnectionError lastConnectionError;

    /// <summary>
    /// Enforces the connection timeout <see cref="disableConnectionTimeout"/>.
    /// </summary>
    private CancellationTokenSource connectTimeoutCTS;

    private CancellationTokenSource readCTS;
    private CancellationTokenSource sendCTS;
    private CancellationTokenSource tlsUpgradeCTS;
    private CancellationTokenSource connectCTS;
    private readonly SemaphoreSlim CONNECT_DISCONNECT_SEMA = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim WRITE_SEMA = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim READ_SEMA = new SemaphoreSlim(1, 1);
    private bool disposed;

    public delegate void NewDataReceivedEventHandler(TcpConnection sender, NewDataReceivedEventArgs args);

    public event NewDataReceivedEventHandler NewDataReceived;

    #endregion

    //--------------------------------------------------------Constructor:----------------------------------------------------------------\\

    #region --Constructors--

    public TcpConnection(XMPPAccount account) : base(account)
    {
    }

    #endregion

    //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\

    #region --Set-, Get- Methods--

    // public StreamSocketInformation GetSocketInfo()
    // {
    //     return socket?.Information;
    // }

    #endregion

    //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\

    #region --Misc Methods (Public)--

    public async Task ConnectAsync()
    {
      if (disposed)
      {
        Logger.Debug(LOGGER_TAG + "Canceling ConnectAsync since we are disposed.");
        return;
      }

      await CONNECT_DISCONNECT_SEMA.WaitAsync();
      if (state != ConnectionState.CONNECTED || state != ConnectionState.CONNECTING)
      {
        SetState(ConnectionState.CONNECTING);
        connectCTS = new CancellationTokenSource();
        try
        {
          bool result = await ConnectInternalAsync(connectCTS.Token);
          if (result)
          {
            StartReaderTask();
          }
        }
        catch (TaskCanceledException)
        {
          Logger.Debug(LOGGER_TAG + "ConnectAsync got canceled.");
          SetState(ConnectionState.DISCONNECTED);
          return;
        }
      }

      CONNECT_DISCONNECT_SEMA.Release();
    }

    public async Task DisconnectAsync()
    {
      CancelConnectionAttempt();
      if (state == ConnectionState.CONNECTED || state == ConnectionState.CONNECTING)
      {
        await CONNECT_DISCONNECT_SEMA.WaitAsync();
        SetState(ConnectionState.DISCONNECTING);
        StopReaderTask();
        Cleanup();
        SetState(ConnectionState.DISCONNECTED);
        CONNECT_DISCONNECT_SEMA.Release();
      }
    }

    public async Task UpgradeToTlsAsync()
    {
      await CONNECT_DISCONNECT_SEMA.WaitAsync();
      if (state != ConnectionState.CONNECTED)
      {
        Logger.Error($"Unable to upgrade to TLS since the state is not connected: {state}");
        CONNECT_DISCONNECT_SEMA.Release();
        return;
      }

      DateTime duration = DateTime.Now;
      try
      {
        tlsUpgradeCTS = disableTlsUpgradeTimeout
            ? new CancellationTokenSource()
            : new CancellationTokenSource(TimeSpan.FromMilliseconds(TLS_UPGRADE_TIMEOUT_MS));

        var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false);

        await sslStream.AuthenticateAsClientAsync(account.user.domainPart, null,
                System.Security.Authentication.SslProtocols.Tls12, checkCertificateRevocation: true)
            .WaitAsync(tlsUpgradeCTS.Token);

        // After successful TLS upgrade:
        sslNetworkStream = sslStream;
        reader = new StreamReader(sslNetworkStream, Encoding.UTF8, leaveOpen: true);
        writer = new StreamWriter(sslNetworkStream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
      }
      catch (Exception e)
      {
        var elapsedMs = DateTime.Now.Subtract(duration).TotalMilliseconds;
        lastConnectionError = new ConnectionError(SocketErrorStatus.Unknown,
            $"TLS upgrade failed after {elapsedMs} ms with: {e.Message}");

        SetState(ConnectionState.ERROR, lastConnectionError);
        throw;
      }
      finally
      {
        CONNECT_DISCONNECT_SEMA.Release();
      }
    }


    public async void Dispose()
    {
      await DisconnectAsync();
      disposed = true;
    }

    public async Task<bool> SendAsync(string s)
    {
      if (state != ConnectionState.CONNECTED || writer is null)
      {
        return false;
      }

      try
      {
        await WRITE_SEMA.WaitAsync();

        using var sendCTS = new CancellationTokenSource(SEND_TIMEOUT_MS);

        try
        {
          await writer.WriteAsync(s.AsMemory(), sendCTS.Token);
          await writer.FlushAsync(sendCTS.Token);

          Logger.Trace($"{LOGGER_TAG}Send to ({account.serverAddress}): {s}");
          return true;
        }
        catch (OperationCanceledException)
        {
          lastConnectionError =
              new ConnectionError(ConnectionErrorCode.SENDING_FAILED, "Send operation timed out");
          Logger.Error($"{LOGGER_TAG}Failed to send - {lastConnectionError.ERROR_MESSAGE}: {s}");
          return false;
        }
        catch (Exception ex)
        {
          lastConnectionError = new ConnectionError(ConnectionErrorCode.SENDING_FAILED, ex.Message);
          Logger.Error($"{LOGGER_TAG}Failed to send - {lastConnectionError.ERROR_MESSAGE}: {s}", ex);
          return false;
        }
      }
      finally
      {
        WRITE_SEMA.Release();
      }
    }


    public async Task<TcpReadResult> ReadAsync()
    {
      await READ_SEMA.WaitAsync();
      if (state != ConnectionState.CONNECTED || reader is null)
      {
        READ_SEMA.Release();
        return new TcpReadResult(TcpReadState.FAILURE, null);
      }

      StringBuilder data = new StringBuilder();
      char[] buffer = new char[BUFFER_SIZE];
      int readCount;

      try
      {
    
        // Read the first batch
        readCount = await reader.ReadAsync(buffer, 0, BUFFER_SIZE);
        READ_SEMA.Release();

        if (readCount <= 0)
        {
          return new TcpReadResult(TcpReadState.END_OF_STREAM, null);
        }

        data.Append(buffer, 0, readCount);

        // Continue reading while buffer is full and connection is active
        while (readCTS != null && !readCTS.IsCancellationRequested &&
               state == ConnectionState.CONNECTED && readCount >= BUFFER_SIZE)
        {
          readCount = await reader.ReadAsync(buffer, 0, BUFFER_SIZE);

          if (readCount > 0)
          {
            data.Append(buffer, 0, readCount);
          }
          else
          {
            break;
          }
        }
      }
      catch (OperationCanceledException)
      {
        return new TcpReadResult(TcpReadState.FAILURE, null);
      }
      catch (Exception)
      {
        return new TcpReadResult(TcpReadState.FAILURE, null);
      }

      return new TcpReadResult(TcpReadState.SUCCESS, data.ToString());
    }


    /// <summary>
    /// Performs a DNS SRV lookup for the given host and returns a list of <see cref="SrvRecord"/> objects for the "xmpp-client" service.
    /// </summary>
    /// <param name="host">The host the lookup should be performed for.</param>
    /// <returns>A list of <see cref="SrvRecord"/> objects ordered by their priority or an empty list of an error occurred or no entries were found.</returns>
    public static async Task<List<SrvRecord>> DnsSrvLookupAsync(string host)
    {
      Logger.Info(LOGGER_TAG + "Performing DNS SRV lookup for: " + host);
      LookupClient lookup = new LookupClient();
      try
      {
        IDnsQueryResponse response = await lookup.QueryAsync(host, QueryType.SRV);
        if (response is DnsQueryResponse dnsResponse)
        {
          IEnumerable<SrvRecord> records = dnsResponse.AllRecords.Where((record) => record is SrvRecord)
              .Select((record) => record as SrvRecord);
          List<SrvRecord> list = records.ToList();
          list.Sort((a, b) => a.Priority.CompareTo(b.Priority));
          return list;
        }
      }
      catch (Exception e)
      {
        Logger.Error(LOGGER_TAG + "Failed to look up DNS SRV record for: " + host, e);
      }

      return new List<SrvRecord>();
    }

    #endregion

    #region --Misc Methods (Private)--

    private void CancelConnectionAttempt()
    {
      if (!disposed)
      {
        connectCTS?.Cancel();
      }
    }

    private void Cleanup()
    {

      // Cancel and dispose all CancellationTokenSources
      connectTimeoutCTS?.Cancel();
      connectTimeoutCTS?.Dispose();
      connectTimeoutCTS = null;

      connectCTS?.Cancel();
      connectCTS?.Dispose();
      connectCTS = null;

      readCTS?.Cancel();
      readCTS?.Dispose();
      readCTS = null;

      sendCTS?.Cancel();
      sendCTS?.Dispose();
      sendCTS = null;

      tlsUpgradeCTS?.Cancel();
      tlsUpgradeCTS?.Dispose();
      tlsUpgradeCTS = null;

      // Dispose writer
      try
      {
        writer?.Dispose();
        writer = null;
      }
      catch (Exception ex)
      {
        Logger.Warn($"{LOGGER_TAG}Error disposing writer: {ex.Message}");
      }

      // Dispose reader
      try
      {
        READ_SEMA.Wait();
        reader?.Dispose();
        reader = null;
        READ_SEMA.Release();
      }
      catch (Exception ex)
      {
        Logger.Warn($"{LOGGER_TAG}Error disposing reader: {ex.Message}");
      }

      // Dispose socket
      try
      {
        tcpClient?.Dispose();
        tcpClient = null;
      }
      catch (Exception ex)
      {
        Logger.Warn($"{LOGGER_TAG}Error disposing socket: {ex.Message}");
      }
    }

    public static bool IsInternetAvailable()
    {
      try
      {
        using (var client = new HttpClient())
        {
          var response = client.GetAsync("https://www.google.com").Result;
          return response.IsSuccessStatusCode;
        }
      }
      catch
      {
        return false;
      }
    }

    private async Task<bool> ConnectInternalAsync(CancellationToken token)
    {
      for (int i = 1; i <= MAX_CONNECTION_ATTEMPTS; i++)
      {
        token.ThrowIfCancellationRequested();

        if (state != ConnectionState.CONNECTING)
        {
          Logger.Debug($"{LOGGER_TAG}Connection attempt canceled.");
          return false;
        }

        if (!IsInternetAvailable())
        {
          lastConnectionError = new ConnectionError(ConnectionErrorCode.NO_INTERNET);
          SetState(ConnectionState.ERROR, lastConnectionError);
          Logger.Warn($"{LOGGER_TAG}Unable to connect to {account.serverAddress} - no internet!");
          return false;
        }

        Logger.Debug($"{LOGGER_TAG}Starting connection attempt {i}...");

        try
        {
          using var connectTimeoutCTS = disableConnectionTimeout
              ? CancellationTokenSource.CreateLinkedTokenSource(token)
              : CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(CONNECTION_TIMEOUT_MS).Token);

          tcpClient = new TcpClient();

          var connectTask = tcpClient.ConnectAsync(account.serverAddress, account.port);
          var timeoutTask = Task.Delay(CONNECTION_TIMEOUT_MS, connectTimeoutCTS.Token);

          var completedTask = await Task.WhenAny(connectTask, timeoutTask);

          token.ThrowIfCancellationRequested();

          if (completedTask != connectTask || !tcpClient.Connected)
          {
            throw new TimeoutException("Connection attempt timed out.");
          }

          networkStream = tcpClient.GetStream();
          reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);
          writer = new StreamWriter(networkStream, Encoding.UTF8, leaveOpen: true)
          {
            AutoFlush = true
          };

          // Todo: account.CONNECTION_INFO.socketInfo = tcpClient.Client.RemoteEndPoint?.ToString();

          SetState(ConnectionState.CONNECTED);
          return true;
        }
        catch (OperationCanceledException)
        {
          Logger.Warn($"{LOGGER_TAG}Connection attempt canceled by token.");
          lastConnectionError = new ConnectionError(ConnectionErrorCode.CONNECT_CANCELED);
          SetState(ConnectionState.ERROR, lastConnectionError);
          return false;
        }
        catch (TimeoutException e)
        {
          Logger.Error($"{LOGGER_TAG}Attempt {i} to connect to {account.serverAddress} timed out.", e);
          lastConnectionError = new ConnectionError(ConnectionErrorCode.CONNECT_TIMEOUT, e.Message);
        }
        catch (Exception e)
        {
          OnConnectionError(e, i);
        }

        if (state == ConnectionState.CONNECTING)
        {
          if (i < MAX_CONNECTION_ATTEMPTS)
          {
            Logger.Info($"{LOGGER_TAG}Waiting before retrying connection...");
            await Task.Delay(CONNECTION_ATTEMPT_DELAY, token);
          }
          else
          {
            SetState(ConnectionState.ERROR, lastConnectionError);
          }
        }
      }

      return false;
    }



    private void StopReaderTask()
    {
      if (readCTS != null && !readCTS.IsCancellationRequested)
      {
        readCTS.Cancel();
      }
    }

    private void StartReaderTask()
    {
      StopReaderTask(); // Ensure no other reader task is running
      readCTS = new CancellationTokenSource();

      try
      {
        Task.Run(async () =>
        {
          int lastReadingFailedCount = 0;
          int errorCount = 0;
          DateTime lastReadingFailed = DateTime.MinValue;

          while (state == ConnectionState.CONNECTED && errorCount < 3)
          {
            try
            {
              var readResult = await ReadAsync();

              switch (readResult.STATE)
              {
                case TcpReadState.SUCCESS:
                  lastReadingFailedCount = 0;
                  errorCount = 0;
                  Logger.Trace($"{LOGGER_TAG}Received from ({account.serverAddress}): {readResult.DATA}");

                  NewDataReceived?.Invoke(this, new NewDataReceivedEventArgs(readResult.DATA));
                  break;

                case TcpReadState.FAILURE:
                  if (lastReadingFailedCount++ == 0)
                    lastReadingFailed = DateTime.Now;

                  double secondsSinceFirstFailure = (DateTime.Now - lastReadingFailed).TotalSeconds;
                  if (lastReadingFailedCount > 5 && secondsSinceFirstFailure < 1)
                  {
                    lastConnectionError = new ConnectionError(ConnectionErrorCode.READING_LOOP);
                    errorCount = int.MaxValue;
                  }
                  break;

                case TcpReadState.END_OF_STREAM:
                  Logger.Info($"{LOGGER_TAG}Socket closed by peer: {account.serverAddress}");
                  await DisconnectAsync();
                  return;
              }
            }
            catch (OperationCanceledException)
            {
              lastConnectionError = new ConnectionError(ConnectionErrorCode.READING_CANCELED);
              errorCount++;
            }
            catch (IOException ioEx)
            {
              lastConnectionError = new ConnectionError(ConnectionErrorCode.CONNECT_CANCELED, ioEx.Message);
              errorCount = int.MaxValue;
            }
            catch (Exception ex)
            {
              lastConnectionError = new ConnectionError(ConnectionErrorCode.UNKNOWN, ex.Message);
              errorCount++;
            }
          }

          if (errorCount >= 3)
          {
            SetState(ConnectionState.ERROR, lastConnectionError);
          }
        }, readCTS.Token);
      }
      catch (OperationCanceledException)
      {
        // Reader task got canceled
      }
    }

    private void OnConnectionError(Exception e, int connectionTry)
    {
      Logger.Error($"{LOGGER_TAG}{connectionTry} try to connect to {account?.serverAddress} failed:", e);

      var baseEx = e.GetBaseException();
      ConnectionErrorCode errorCode;

      if (baseEx is SocketException socketEx)
      {
        // Map specific SocketErrorCode to your ConnectionErrorCode enum
        switch (socketEx.SocketErrorCode)
        {
          case SocketError.TimedOut:
            errorCode = ConnectionErrorCode.CONNECT_TIMEOUT;
            break;
          case SocketError.ConnectionRefused:
            errorCode = ConnectionErrorCode.CONNECT_TIMEOUT;
            break;
          case SocketError.NetworkDown:
          case SocketError.NetworkUnreachable:
            errorCode = ConnectionErrorCode.SERVER_ERROR;
            break;
          case SocketError.ConnectionReset:
            errorCode = ConnectionErrorCode.CONNECT_CANCELED;
            break;
          default:
            errorCode = ConnectionErrorCode.SOCKET_ERROR;
            break;
        }
      }
      else
      {
        errorCode = ConnectionErrorCode.UNKNOWN;
      }

      lastConnectionError = new ConnectionError(errorCode, baseEx.Message);
    }

    #endregion

    #region --Misc Methods (Protected)--

    #endregion

    //--------------------------------------------------------Events:---------------------------------------------------------------------\\

    #region --Events--

    #endregion
  }
}