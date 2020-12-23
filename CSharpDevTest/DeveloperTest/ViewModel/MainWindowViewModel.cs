using DeveloperTest.Model;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DeveloperTest.ViewModel
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly IMailService mailService;

        private string servername;
        private string port;
        private string username;
        private string password;
        private ConnectionType selectedConnection = ConnectionType.IMAP;
        private EncryptionType selectedEncryption = EncryptionType.SSL_TLS;

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable ConnectionTypes => Enum.GetValues(typeof(ConnectionType));
        public IEnumerable EncryptionTypes => Enum.GetValues(typeof(EncryptionType));

        public ConnectionType SelectedConnection
        {
            get { return selectedConnection; }
            set
            {
                if (value == selectedConnection)
                    return;

                selectedConnection = value;
                OnPropertyChanged();
            }
        }

        public EncryptionType SelectedEncryption
        {
            get { return selectedEncryption; }
            set
            {
                if (value == selectedEncryption)
                    return;

                selectedEncryption = value;
                OnPropertyChanged();
            }
        }

        public string Servername
        {
            get { return servername; }
            set
            {
                if (value == servername)
                    return;

                servername = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartCommand));
            }
        }

        public string Port
        {
            get { return port; }
            set
            {
                if (value == port)
                    return;
                if (!Regex.IsMatch(value, "^\\d+$"))
                    return;

                port = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartCommand));
            }
        }

        public string Username
        {
            get { return username; }
            set
            {
                if (value == username)
                    return;

                username = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartCommand));
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (value == password)
                    return;

                password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartCommand));
            }
        }

        public ICommand StartCommand => new RelayCommand(Start, CanStart);

        public MainWindowViewModel()
        {
            mailService = new MailService();
        }

        private bool CanStart(object obj)
        {
            return !string.IsNullOrEmpty(Password) &&
                    !string.IsNullOrEmpty(Username) &&
                    !string.IsNullOrEmpty(Servername) &&
                    !string.IsNullOrEmpty(Port);
        }

        private void Start(object obj)
        {
            int port = int.Parse(Port);
            mailService.ConnectAsync(Servername, port, Username, Password);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
