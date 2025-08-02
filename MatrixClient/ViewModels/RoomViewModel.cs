using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using matrix_dotnet.Api;
using matrix_dotnet.Client;

namespace MatrixClient.ViewModels;

public partial class RoomViewModel :ViewModelBase
{
    
    public RoomViewModel()
    {}
    [ObservableProperty]
    public string _roomId;

    [ObservableProperty]
    public string _joinRule;

    [ObservableProperty]
    public string _roomName;

    public RoomViewModel(string toString, ImmutableDictionary<StateKey, EventContent> inviteStateValue)
    {
        throw new System.NotImplementedException();
    }
    public ConcurrentQueue<MatrixEvent> EventQueue { get; } = new();
    private bool _isProcessing;

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
}

public class MatrixEvent
{
    public StateKey StateKey { get; set; }
    public EventContent EventContent { get; set; }
}