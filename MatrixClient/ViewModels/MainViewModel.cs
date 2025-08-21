using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager;
using Storage;
using Storage.Contexts;
using Storage.Models.Account;
using System;

//using Matrix.Business;
//using matrix_dotnet.Api;
//using matrix_dotnet.Client;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using XmppApi;
using XmppApi.Network;
using XmppApi.Network.XML;
using XmppApi.Network.XML.Messages;

namespace MatrixClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
  public string Greeting => "Welcome to Avalonia!";
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
    var test = "test@bobbin.synology.me";

    JidModel jid = new JidModel
    {
      localPart = Utils.getJidLocalPart(test),
      domainPart = Utils.getJidDomainPart(test),
      resourcePart = Utils.getRandomResourcePart()
    };
    AccountModel account = new AccountModel(jid, "color");
    account.omemoInfo.GenerateOmemoKeys();

    // Look up the DNS SRV record:
    SRVLookupResult result = await XMPPAccount.dnsSrvLookupAsync(jid.domainPart);
    if (result.SUCCESS)
    {
      account.server.address = result.SERVER_ADDRESS;
      account.server.port = result.PORT;
    }

    account = await SaveAccount(account);

    var t = ConnectionHandler.INSTANCE.GetClients();
    t.First().client.xmppClient.NewChatMessage += XmppClient_NewChatMessage;
    t.First().client.xmppClient.NewRoosterMessage += XmppClient_NewRoosterMessage;
  }

  private void XmppClient_NewRoosterMessage(XmppApi.Network.XML.Messages.IMessageSender sender, XmppApi.Network.Events.NewValidMessageEventArgs args)
  {
    var mess = args.MESSAGE as RosterResultMessage;
    if(mess == null)
    {
      return;
    }

    // Process the roster items
    mess.ITEMS
      .OfType<RosterItem>()
      .ToList()
      .ForEach(item =>
      {
        var room = Rooms.FirstOrDefault(x => x.RoomName.Equals(item.JID, StringComparison.OrdinalIgnoreCase));
        if (room == null)
        {
          room = new RoomViewModel(item.JID);
         
          Rooms.Add(room);
         
        }
        //  }
      });
  }

  private void XmppClient_NewChatMessage(XMPPClient client, XmppApi.Network.Events.NewChatMessageEventArgs args)
  {
    //throw new NotImplementedException();
  }

  private static async Task<AccountModel> SaveAccount(AccountModel dbAcount)
  {
    // Cleanup password vault:
    using (MainDbContext ctx = new MainDbContext())
    {
      // Vault.DeleteAllVaults(ctx.Accounts.Select(a => a.bareJid).ToList());
    }

    // Update the password:
    XMPPAccount account = dbAcount.ToXMPPAccount();
    account.user.password = "tesT923@45";
    await Vault.StorePassword(account);

    // Push:
    //MODEL.Account.push.state = Settings.GetSettingBoolean(SettingsConsts.PUSH_ENABLED) ? PushState.ENABLING : PushState.DISABLED;

    //MODEL.Account.Add();
    ConnectionHandler.INSTANCE.AddAccount(dbAcount);


    return dbAcount;
  }    //_client = new XmppClient(conf =>
    //    {
    //        conf.UseSocketTransport();
    //        //conf.UseWebSocketTransport();
    //        conf.AutoReconnect = true;

    //        // when your server dow not support SRV records or
    //        // XEP-0156 Discovering Alternative XMPP Connection Methods
    //        // then you need to supply host and port for the connection as well.
    //        // See docs => Host disconvery
    //    }
    //)
    //{
    //    Jid = "test@bobbin.synology.me",
    //    Password = "tesT923@45" //  Jid = "test@bobbin.synology.me",
    //    //  Password = "tesT923@45"
    //};
    //_client.StateChanged.Subscribe(v => { Debug.WriteLine($"State changed: {v}"); });

    //_client
    //    .XmppXElementReceived
    //    .Where(el => el is XmppDotNet.Xmpp.Base.Presence)
    //    .Subscribe(async el => { await ProcessPrescence((XmppDotNet.Xmpp.Base.Presence)el); });

    //_client
    //    .XmppXElementReceived
    //    .Where(el => el is XmppDotNet.Xmpp.Base.Message)
    //    .Subscribe(async el => { await ProcessMessage((XmppDotNet.Xmpp.Base.Message)el); });

    //_client
    //    .XmppXElementReceived
    //    .Where(el => el is XmppDotNet.Xmpp.Base.Iq)
    //    .Subscribe(el => { Debug.WriteLine(el.ToString()); });

    //_client
    //    .StateChanged
    //    .Where(s => s == SessionState.Binded)
    //    .Subscribe(async v =>
    //    {
    //        var roster = await _client.RequestRosterAsync();
    //        await ProcessRoster(roster);
    //        await _client.SendPresenceAsync(Show.Chat, "free for chat");
    //    });

    //_omemoClient = new OmemoClient(_client, deviceId: 12345);

    //await _client.ConnectAsync();

    //await _omemoClient.PublishBundle();
  

  //  private async Task ProcessRoster(XmppDotNet.Xmpp.Client.Iq rosterIqResult)
  //{
  //  var rosterItems
  //      = rosterIqResult
  //          .Query
  //          .Cast<Roster>()
  //          .GetRoster();

  //  // enumerate over the items and build your contact list or GUI
  //  foreach (var ri in rosterItems)
  //  {
  //    var room = Rooms.FirstOrDefault(x => x.RoomId.Bare.Equals(ri.Jid.Bare, StringComparison.OrdinalIgnoreCase));
  //    if (room == null)
  //    {
  //      room = new RoomViewModel(ri, _client, _omemoClient);
  //      await room.SetupOmemoSession();
  //      Rooms.Add(room);
  //      await room.LoadHistory();
  //    }
  //  }
  //}


  //private async Task ProcessMessage(XmppDotNet.Xmpp.Base.Message el)
  //{
  //  RoomViewModel room;
  //  switch (el.Type)
  //  {
  //    case MessageType.Chat:

  //      await ProcessChatMessage(el);

  //      Debug.WriteLine($"Chat message from {el.From}: {el.Body}");
  //      break;
  //    case MessageType.GroupChat:
  //      Debug.WriteLine($"Group chat message from {el.From}: {el.Body}");
  //      break;
  //    case MessageType.Normal:
  //      Debug.WriteLine($"Normal message from {el.From}: {el.Body}");
  //      break;
  //    case MessageType.Error:
  //      Debug.WriteLine($"Error message from {el.From}: {el.Body}");
  //      break;
  //    default:
  //      Debug.WriteLine($"Unknown message type from {el.From}: {el.Body}");
  //      break;
  //  }

  //  Debug.WriteLine(el.ToString());
  //}

  //private async Task ProcessChatMessage(XmppDotNet.Xmpp.Base.Message el)
  //{

  //  if (_omemoClient != null)
  //  {
  //    _omemoClient.HandleMessage(el);
  //  }
  //  var room = Rooms.FirstOrDefault(r => r.RoomId.Bare == el.From.Bare);
  //  if (room == null)
  //  {
  //    room = new RoomViewModel(el.From, _client, _omemoClient);
  //    await room.SetupOmemoSession();
  //    Rooms.Add(room);
  //    await room.LoadHistory();
  //  }

  //  room.ProcessMessage(el);
  //}

  //private async Task ProcessPrescence(XmppDotNet.Xmpp.Base.Presence el)
  //{
  //}

  #endregion xmpp
}