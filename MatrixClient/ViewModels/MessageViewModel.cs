using CommunityToolkit.Mvvm.ComponentModel;

namespace MatrixClient.ViewModels;

public partial class MessageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _message;
    
    [ObservableProperty]
    private string _sender;

}