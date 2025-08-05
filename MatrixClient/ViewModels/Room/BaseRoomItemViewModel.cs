using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MatrixClient.ViewModels.Room
{
  public abstract partial class BaseRoomItemViewModel : ViewModelBase
  {
    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private string _sender;

    [ObservableProperty]
    private string _time;

    [ObservableProperty]
    private DateTime? _timestamp;
  }
}
