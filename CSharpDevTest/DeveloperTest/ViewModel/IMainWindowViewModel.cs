using System.ComponentModel;
using System.Windows.Input;

namespace DeveloperTest.ViewModel
{
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        string Servername { get; set; }
        string Port { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        ICommand StartCommand { get; }
    }
}
