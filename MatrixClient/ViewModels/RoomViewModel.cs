using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace MatrixClient.ViewModels;

public partial class RoomViewModel : ObservableRecipient
{
  public string RoomName { get; set; }

  public RoomViewModel(string roomName)
  {
    RoomName = roomName;
  }
}
