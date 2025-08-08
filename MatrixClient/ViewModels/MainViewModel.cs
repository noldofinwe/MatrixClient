using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

//using Matrix.Business;
//using matrix_dotnet.Api;
//using matrix_dotnet.Client;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MatrixClient.Services.Omemo;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Presence;
using XmppDotNet.Extensions.Client.Roster;
using XmppDotNet.Transport.Socket;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Roster;

namespace MatrixClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    private XmppClient _client;
    private OmemoClient _omemoClient;
    public ObservableCollection<RoomViewModel> Rooms { get; set; }

    [ObservableProperty] private RoomViewModel? _selectedRoom;

    public MainViewModel()
    {
        Rooms = new();
        // Constructor logic can go here if needed
    }

    #region xmpp

    [RelayCommand]
    public async Task LoginXmpp()
    {

        _client = new XmppClient(conf =>
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
            Password = "tesT923@45" //  Jid = "test@bobbin.synology.me",
            //  Password = "tesT923@45"
        };
        _client.StateChanged.Subscribe(v => { Debug.WriteLine($"State changed: {v}"); });

        _client
            .XmppXElementReceived
            .Where(el => el is XmppDotNet.Xmpp.Base.Presence)
            .Subscribe(async el => { await ProcessPrescence((XmppDotNet.Xmpp.Base.Presence)el); });

        _client
            .XmppXElementReceived
            .Where(el => el is XmppDotNet.Xmpp.Base.Message)
            .Subscribe(async el => { await ProcessMessage((XmppDotNet.Xmpp.Base.Message)el); });

        _client
            .XmppXElementReceived
            .Where(el => el is XmppDotNet.Xmpp.Base.Iq)
            .Subscribe(el => { Debug.WriteLine(el.ToString()); });

        _client
            .StateChanged
            .Where(s => s == SessionState.Binded)
            .Subscribe(async v =>
            {
                var roster = await _client.RequestRosterAsync();
                await ProcessRoster(roster);
                await _client.SendPresenceAsync(Show.Chat, "free for chat");
            });

        _omemoClient = new OmemoClient(_client, deviceId: 12345);

        await _client.ConnectAsync();
        
        await _omemoClient.PublishBundle();
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
            var room = Rooms.FirstOrDefault(x => x.RoomId.Bare.Equals(ri.Jid.Bare, StringComparison.OrdinalIgnoreCase));
            if (room == null)
            {
                room = new RoomViewModel(ri, _client, _omemoClient);
                await room.SetupOmemoSession();
                Rooms.Add(room);
                await room.LoadHistory();
            }
        }
    }


    private async Task ProcessMessage(XmppDotNet.Xmpp.Base.Message el)
    {
        RoomViewModel room;
        switch (el.Type)
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

        if (_omemoClient != null)
        {
            _omemoClient.HandleMessage(el);
        }
        var room = Rooms.FirstOrDefault(r => r.RoomId.Bare == el.From.Bare);
        if (room == null)
        {
            room = new RoomViewModel(el.From, _client, _omemoClient);
            await room.SetupOmemoSession();
            Rooms.Add(room);
            await room.LoadHistory();
        }

        room.ProcessMessage(el);
    }

    private async Task ProcessPrescence(XmppDotNet.Xmpp.Base.Presence el)
    {
    }

    #endregion xmpp
}