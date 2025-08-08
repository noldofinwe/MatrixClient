using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatrixClient.Services;
using MatrixClient.Services.Omemo;
using MatrixClient.ViewModels.Room;
using XmppDotNet;
using XmppDotNet.Extensions.Client.Message;
using XmppDotNet.Extensions.Client.Roster;
using XmppDotNet.Xmpp.Base;

namespace MatrixClient.ViewModels;

public partial class RoomViewModel : ObservableRecipient
{
  private MamService _mamService = new();
  public RoomViewModel(RosterItem key, XmppClient client, OmemoClient omemoClient)
  {
    _mamService = new MamService();
    _client = client;
    InvitedRoom = false;
    RoomId = key.Jid;
    RoomName = key.Name ?? key.Jid.Bare.ToString();
    FriendlyName = key.Jid.Local;
    Avatar = AvatarService.GetOrCreateAvatar(key.Jid.Bare);
    _omemoClient = omemoClient;
  }

  public RoomViewModel(Jid jid, XmppClient client, OmemoClient omemoClient)
  {
    RoomId = jid;
    RoomName = jid.Bare.ToString();
    InvitedRoom = true;
    _client = client;
    _omemoClient = omemoClient;
  }

  public async Task SetupOmemoSession()
  {
    var pubSubManager = new PubSubManager(_client);
    var bundle = await pubSubManager.GetBundleAsync(RoomId, _omemoClient.DeviceId);
    _omemoClient.CreateSession(RoomId, bundle);
  }

  public async Task<bool> LoadHistory()
  {
    var result = await _mamService.RequestLastChatMessagesFromArchive(_client, RoomId.Bare, 100);

    if (result.IsSuccess)
    {
      foreach(var forwarded in result.Messages)
      {
        if(forwarded.Message.From.Bare == _client.Jid.Bare)
        {
          _omemoClient.HandleMessage((forwarded.Message));
          
          var messageViewModel = new OwnMessageViewModel
          {
            Message = forwarded.Message.Body,
            Sender = forwarded.Message.From.Local,
            Time = forwarded.Delay?.Stamp.ToString("HH:mm") ?? string.Empty,

            Timestamp = forwarded.Delay?.Stamp

          };
          AddRoomMessage(messageViewModel);
        }
        else
        {
          var messageViewModel = new MessageViewModel
          {
            Message = forwarded.Message.Body,
            Sender = forwarded.Message.From.Local,
            Time = forwarded.Delay?.Stamp.ToString("HH:mm") ?? string.Empty,
             Timestamp = forwarded.Delay?.Stamp
          };
          AddRoomMessage(messageViewModel);
        }

      
      }
    }

    AddRoomMessage(new DateViewModel { Message = "History" });
    return result.Messages.Any();
  }


  [ObservableProperty]
  private Jid _roomId;

  [ObservableProperty]
  private string _joinRule;

  [ObservableProperty]
  private string _roomName;

  [ObservableProperty]
  private string _messageToSend;

  [ObservableProperty]
  private string _friendlyName;

  [ObservableProperty]
  private string _activity;

  [ObservableProperty]
  private bool _invitedRoom;

  [ObservableProperty]
  private Bitmap _avatar;

  [ObservableProperty]
  private ObservableCollection<BaseRoomItemViewModel> _roomMessages = new();
  private XmppClient _client;
  private readonly OmemoClient _omemoClient;

  [RelayCommand]
  public async Task AddToRoster()
  {
    var result = await _client.AddRosterItemAsync(RoomId);
    InvitedRoom = false;
  }

  [RelayCommand]
  public async Task SendMessage()
  {
    if (string.IsNullOrWhiteSpace(MessageToSend)) return;
    var message = new OwnMessageViewModel
    {
      Message = MessageToSend,
      Sender = _client.Jid.Local.ToString(),
      Time = DateTime.Now.ToString("HH:mm"),
    };
    AddRoomMessage(message);

    // Here you would send the message to the server
    // await _client.SendMessageAsync(RoomId, MessageToSend);
    await _client.SendChatMessageAsync(RoomId, MessageToSend);
    MessageToSend = string.Empty;
  }

  internal void ProcessMessage(Message el)
  {

    Activity = el.Chatstate.ToString();
    
    if (string.IsNullOrWhiteSpace(el.Body)) return;


    var message = new MessageViewModel
    {
      Message = el.Body,
      Sender = _client.Jid.Local.ToString(),
      Time = DateTime.Now.ToString("HH:mm"),
    };
    AddRoomMessage(message);
  }


  public void AddRoomMessage(BaseRoomItemViewModel message)
  {
     var lastMessage = RoomMessages.LastOrDefault();
    if(lastMessage != null && lastMessage.Timestamp.HasValue && message.Timestamp != null)
    {

      if(lastMessage.Timestamp.Value.Day != message.Timestamp.Value.Day)
      {
        RoomMessages.Add(new DateViewModel
        {
          Message = message.Timestamp.Value.ToString("yyyy-MM-dd")
        });
      }
    }
    if(lastMessage == null && message.Timestamp != null)
    {
      RoomMessages.Add(new DateViewModel
      {
        Message = message.Timestamp.Value.ToString("yyyy-MM-dd")
      });
    }
    RoomMessages.Add(message);

  }
}
