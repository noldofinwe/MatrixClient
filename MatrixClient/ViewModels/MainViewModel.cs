
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Matrix.Sdk.Core.Infrastructure.Dto.Sync;

//using Matrix.Business;
//using matrix_dotnet.Api;
//using matrix_dotnet.Client;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Message;
using XmppDotNet.Extensions.Client.Presence;
using XmppDotNet.Extensions.Client.Roster;
using XmppDotNet.Transport.Socket;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Base;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.MessageArchiveManagement;
using XmppDotNet.Xmpp.ResultSetManagement;
using XmppDotNet.Xmpp.Roster;
using XmppDotNet.Xmpp.XData;

namespace MatrixClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
  public string Greeting => "Welcome to Avalonia!";
  private XmppClient _client;
  public ObservableCollection<RoomViewModel> Rooms { get; set; }

  [ObservableProperty]
  private RoomViewModel? _selectedRoom;

  public MainViewModel()
  {
    Rooms = new();
    // Constructor logic can go here if needed



  }

  #region xmpp

  [RelayCommand]
  public async Task LoginXmpp()
  {
    _client = new XmppClient(
        conf =>
        {
          conf.UseSocketTransport();
          //conf.UseWebSocketTransport();
          conf.AutoReconnect = true;

          // when your server dow not support SRV records or
          // XEP-0156 Discovering Alternative XMPP Connection Methods
          // then you need to supply host and port for the connection as well.
          // See docs => Host disconvery
        }
    )
    {
    Jid = "test@bobbin.synology.me",
    Password = ""//  Jid = "test@bobbin.synology.me",
                           //  Password = "tesT923@45"
    };
    _client.StateChanged.Subscribe(v => {
      Debug.WriteLine($"State changed: {v}");
    });

    _client
        .XmppXElementReceived
        .Where(el => el is XmppDotNet.Xmpp.Base.Presence)
        .Subscribe(el =>
        {
          ProcessPrescence((XmppDotNet.Xmpp.Base.Presence)el);
        });

    _client
        .XmppXElementReceived
        .Where(el => el is XmppDotNet.Xmpp.Base.Message)
        .Subscribe(async el =>
        {
          await ProcessMessage((XmppDotNet.Xmpp.Base.Message) el);
        });

    _client
        .XmppXElementReceived
        .Where(el => el is XmppDotNet.Xmpp.Base.Iq)
        .Subscribe(el =>
        {
          Debug.WriteLine(el.ToString());
        });

    _client
        .StateChanged
        .Where(s => s == SessionState.Binded)
        .Subscribe(async v =>
        {
          var roster = await _client.RequestRosterAsync();
          await ProcessRoster(roster);
          await _client.SendPresenceAsync(Show.Chat, "free for chat");
        });


    await _client.ConnectAsync();
  }

  private async Task ProcessRoster(XmppDotNet.Xmpp.Client.Iq rosterIqResult)
  {
    var rosterItems
    = rosterIqResult
        .Query
        .Cast<Roster>()
        .GetRoster();

    // enumerate over the items and build your contact list or GUI
    foreach (var ri in rosterItems)
    {
      var room = new RoomViewModel(ri, _client);
      Rooms.Add(room);
      await room.LoadHistory();
    }
  }

  private async Task ProcessMessage(XmppDotNet.Xmpp.Base.Message el)
  {
    RoomViewModel room;
    switch(el.Type)
      {
      case MessageType.Chat:

        await ProcessChatMessage(el);

        Debug.WriteLine($"Chat message from {el.From}: {el.Body}");
        break;
      case MessageType.GroupChat:
        Debug.WriteLine($"Group chat message from {el.From}: {el.Body}");
        break;
      case MessageType.Normal:
        Debug.WriteLine($"Normal message from {el.From}: {el.Body}");
        break;
      case MessageType.Error:
        Debug.WriteLine($"Error message from {el.From}: {el.Body}");
        break;
      default:
        Debug.WriteLine($"Unknown message type from {el.From}: {el.Body}");
        break;
    }
    Debug.WriteLine(el.ToString());
  }

  private async Task ProcessChatMessage(XmppDotNet.Xmpp.Base.Message el)
  {
    var room = Rooms.FirstOrDefault(r => r.RoomId.Bare == el.From.Bare);
    if (room == null)
    {
      room = new RoomViewModel(el.From, _client);
      Rooms.Add(room);
     
    }
    await room.LoadHistory();
    room.ProcessMessage(el);
  }

  private void ProcessPrescence(XmppDotNet.Xmpp.Base.Presence el)
  {
    Debug.WriteLine(el.ToString());
  }
  #endregion xmpp

  #region Matrix
  [RelayCommand]
  public async Task Click()
  {
    //_client = new matrix_dotnet.Client.MatrixClient("https://matrix.bobbin.synology.me");
    //await _client.PasswordLogin("test", "test", initialDeviceDisplayName: "FreeReal development");

    //using (FileStream stream = File.Create("client.json"))
    //{
    //  JsonSerializer.SerializeAsync(stream, _client.ToLoginData()).Wait();
    //}

    //await SyncMatrixRoomsFirstTimeAsync();
    //// foreach (var inviteState in client.InvitiedState)
    //// {
    ////   var i = new RoomViewModel(inviteState.Key.ToString(),  inviteState.Value);
    ////   
    ////   i.RoomId = inviteState.Key.ToString();
    ////   foreach (var s in inviteState.Value)
    ////   {
    ////     i.ProcessEvent(s.Key, s.Value);
    ////     Console.Out.WriteLine($"{s.Key} {s.Value.ToString()} ");
    ////   }
    ////   Rooms.Add(i);
    //// }
    //// client.JoinRoom()
    //await StartSyncLoopAsync();
  }


  //private CancellationTokenSource _cts = new();

  //public async Task StartSyncLoopAsync()
  //{
  //  var timer = new PeriodicTimer(TimeSpan.FromSeconds(5)); // Set sync interval

  //  try
  //  {
  //    while (await timer.WaitForNextTickAsync(_cts.Token))
  //    {
  //      await SyncMatrixRoomsAsync();
  //    }
  //  }
  //  catch (OperationCanceledException)
  //  {
  //    // Gracefully handle cancellation
  //  }
  //}

  //public void StopSyncLoop()
  //{
  //  _cts.Cancel();
  //}

  //private async Task SyncMatrixRoomsAsync()
  //{
  //  await _client.Sync(5000);

  //  // Map updated data to RoomViewModels
  //  foreach (var roomKeyValue in _client.JoinedRooms)
  //  {
  //    var room = Rooms.FirstOrDefault(x => x.RoomId == roomKeyValue.Key.ToString());
  //    if (room == null)
  //    {
  //      room = new RoomViewModel(roomKeyValue.Key.ToString(), _client);
  //      Rooms.Add(room);
  //    }

  //    room.InvitedRoom = false;
  //    await room.ProcessTimeline(roomKeyValue.Value.timeline);
  //  }
  //}

  //private async Task SyncMatrixRoomsFirstTimeAsync()
  //{
  //  await _client.Sync();

  //  // Map updated data to RoomViewModels
  //  foreach (var roomKeyValue in _client.InvitiedState)
  //  {
  //    var room = Rooms.FirstOrDefault(x => x.RoomId == roomKeyValue.Key.ToString());
  //    if (room == null)
  //    {
  //      room = new RoomViewModel(roomKeyValue.Key.ToString(), _client);
  //      Rooms.Add(room);
  //    }

  //  }
  //}
  #endregion Matrix
}
