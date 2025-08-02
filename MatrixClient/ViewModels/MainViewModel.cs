
using CommunityToolkit.Mvvm.Input;
using matrix_dotnet.Api;
using matrix_dotnet.Client;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Matrix.Business;

namespace MatrixClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
  public string Greeting => "Welcome to Avalonia!";
  private matrix_dotnet.Client.MatrixClient _client;
  public ObservableCollection<RoomViewModel> Rooms { get; set; }
  
  [ObservableProperty]
  private RoomViewModel? _selectedRoom;
  
  public MainViewModel()
  {
    Rooms = new();
    // Constructor logic can go here if needed

  

  }

  [RelayCommand]
  public async Task Click()
  {
    _client = new matrix_dotnet.Client.MatrixClient("https://matrix.bobbin.synology.me");
    await _client.PasswordLogin("test", "test", initialDeviceDisplayName: "FreeReal development");

    using (FileStream stream = File.Create("client.json"))
    {
      JsonSerializer.SerializeAsync(stream, _client.ToLoginData()).Wait();
    }

    await SyncMatrixRoomsFirstTimeAsync();
    // foreach (var inviteState in client.InvitiedState)
    // {
    //   var i = new RoomViewModel(inviteState.Key.ToString(),  inviteState.Value);
    //   
    //   i.RoomId = inviteState.Key.ToString();
    //   foreach (var s in inviteState.Value)
    //   {
    //     i.ProcessEvent(s.Key, s.Value);
    //     Console.Out.WriteLine($"{s.Key} {s.Value.ToString()} ");
    //   }
    //   Rooms.Add(i);
    // }
    // client.JoinRoom()
    await StartSyncLoopAsync();
  }
  
  private CancellationTokenSource _cts = new();

  public async Task StartSyncLoopAsync()
  {
    var timer = new PeriodicTimer(TimeSpan.FromSeconds(5)); // Set sync interval

    try
    {
      while (await timer.WaitForNextTickAsync(_cts.Token))
      {
        await SyncMatrixRoomsAsync();
      }
    }
    catch (OperationCanceledException)
    {
      // Gracefully handle cancellation
    }
  }

  public void StopSyncLoop()
  {
    _cts.Cancel();
  }

  private async Task SyncMatrixRoomsAsync()
  {
    await _client.Sync(5000);

    // Map updated data to RoomViewModels
    foreach (var roomKeyValue in _client.JoinedRooms)
    {
      var room = Rooms.FirstOrDefault(x => x.RoomId == roomKeyValue.Key.ToString());
      if (room == null)
      {
        room = new RoomViewModel(roomKeyValue.Key.ToString(), _client);
        Rooms.Add(room);
      }
      SelectedRoom = room;
      await room.ProcessTimeline(roomKeyValue.Value.timeline);
    }
  }
  
  private async Task SyncMatrixRoomsFirstTimeAsync()
  {
    await _client.Sync();

    // Map updated data to RoomViewModels
    foreach (var roomKeyValue in _client.InvitiedState)
    {
      var room = Rooms.FirstOrDefault(x => x.RoomId == roomKeyValue.Key.ToString());
      if (room == null)
      {
        room = new RoomViewModel(roomKeyValue.Key.ToString(), _client);
        Rooms.Add(room);
      }


      await room.ProcessEvents(roomKeyValue.Value);
    }
  }
}
