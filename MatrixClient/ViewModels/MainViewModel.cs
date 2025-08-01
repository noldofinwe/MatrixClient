
using CommunityToolkit.Mvvm.Input;
using matrix_dotnet.Api;
using matrix_dotnet.Client;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatrixClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
  public string Greeting => "Welcome to Avalonia!";
  public MainViewModel()
  {
    // Constructor logic can go here if needed

  

  }

  [RelayCommand]
  public async Task Click()
  {
    var client = new matrix_dotnet.Client.MatrixClient("https://matrix.bobbin.synology.me");
    await client.PasswordLogin("", "", initialDeviceDisplayName: "FreeReal development");

    using (FileStream stream = File.Create("client.json"))
    {
      JsonSerializer.SerializeAsync(stream, client.ToLoginData()).Wait();
    }

    await client.Sync();

    var roomTimeline = client.JoinedRooms[new RoomID("<room_id>")].timeline;
    await foreach (var timelineEvent in roomTimeline.First.EnumerateForward())
    {
      EventWithState ev = timelineEvent.Value;
      if (ev.Event.content is Message message)
        Console.WriteLine($"{ev.GetSender()?.displayname ?? ev.Event.sender}: {message.body}");
    }

  }
}
