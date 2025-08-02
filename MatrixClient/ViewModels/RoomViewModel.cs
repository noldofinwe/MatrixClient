using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using matrix_dotnet.Api;
using matrix_dotnet.Client;
using Timeline = matrix_dotnet.Client.Timeline;

namespace MatrixClient.ViewModels;

public partial class RoomViewModel :ObservableRecipient
{

    public RoomViewModel(string key, matrix_dotnet.Client.MatrixClient client)
    {
        _client = client;
        InvitedRoom = true;
        RoomId = key;
    }
    [ObservableProperty]
    private string _roomId;

    [ObservableProperty]
    private string _joinRule;

    [ObservableProperty]
    private string _roomName;

    [ObservableProperty] 
    private string _messageToSend;
    [ObservableProperty]
    private bool _invitedRoom;

    [ObservableProperty]
    private ObservableCollection<MessageViewModel>  _roomMessages = new();
    
    
    [RelayCommand]
    public async Task JoinRoom()
    {
        var response = await _client.JoinRoom(new RoomID(RoomId));
        InvitedRoom = false;
    }

    [RelayCommand]
    public async Task SendMessage()
    {
        await _client.SendMessage(new RoomID(RoomId), new TextMessage(MessageToSend));
        MessageToSend = string.Empty;
    }
    
    public ConcurrentQueue<MatrixEvent> EventQueue { get; } = new();
    private bool _isProcessing;
    private readonly matrix_dotnet.Client.MatrixClient _client;
    private ITimelineEvent? _lastPoint; 
    public async Task EnqueueEvents(IEnumerable<MatrixEvent> events)
    {
        foreach (var evt in events)
            EventQueue.Enqueue(evt);

        if (!_isProcessing)
            await ProcessEventQueue(); // kick off processing
    }

    private async Task ProcessEventQueue()
    {
        _isProcessing = true;

        while (EventQueue.TryDequeue(out var evt))
        {
            await ProcessEventAsync(evt); // your logic
        }

        _isProcessing = false;
    }



    public async Task ProcessEventAsync(MatrixEvent evt)
    {
        if (evt.EventContent is JoinRules)
        {
            var joinRules = evt.EventContent as JoinRules;
            JoinRule = joinRules.join_rule.ToString(); 
        }
        else if (evt.EventContent is RoomMember)
        {
            var roomMember = evt.EventContent as RoomMember;
            
        }
        else if (evt.EventContent is RoomCreation)
        {
            var roomCreation = evt.EventContent as RoomCreation;
            
        }
            
    }

    public async Task ProcessEvents(ImmutableDictionary<StateKey, EventContent> value)
    {
        await EnqueueEvents(value.Select(x => new MatrixEvent { StateKey = x.Key, EventContent = x.Value }));
    }

    public async Task ProcessTimeline(Timeline roomTimeline)
    {
        if (roomTimeline is null)
            return;
        
        if (_lastPoint == null)
        {
            await ProcessTimelineFirstTime(roomTimeline);
        }

        if (_lastPoint != null)
        {

            if ((await _lastPoint?.Next()) is not null)
            {
                await foreach (var timelineEvent in (await _lastPoint.Next()).EnumerateForward())
                {
                    EventWithState ev = timelineEvent.Value;
                    if (ev.Event.content is Message message)
                        RoomMessages.Add(new MessageViewModel
                            { Message = message.body, Sender = ev.GetSender()?.displayname ?? ev.Event.sender });

                    _lastPoint = timelineEvent;
                }
            }
        }

    }

    private async Task ProcessTimelineFirstTime(Timeline roomTimeline)
    {
        await foreach (var timelineEvent in roomTimeline.First.EnumerateForward()) {
            EventWithState ev = timelineEvent.Value;
            _lastPoint = timelineEvent;
            if (ev.Event.content is Message message)
                RoomMessages.Add(new MessageViewModel
                    { Message = message.body, Sender = ev.GetSender()?.displayname ?? ev.Event.sender });
        }

    }
}

public class MatrixEvent
{
    public StateKey StateKey { get; set; }
    public EventContent EventContent { get; set; }
}