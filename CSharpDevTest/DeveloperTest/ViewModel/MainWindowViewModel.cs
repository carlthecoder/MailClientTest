using DeveloperTest.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeveloperTest.ViewModel
{
    public class MainWindowViewModel : IMainWindowViewModel, IMailObserver
    {
        private readonly IMailService mailService;

        private string servername = "imap.gmail.com";
        private int port = 993;
        private string username = "carl.claessens@gmail.com";
        private string password = "Gm@ilpas13";
        private ConnectionType selectedConnection = ConnectionType.IMAP;
        private EncryptionType selectedEncryption = EncryptionType.SSL_TLS;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MailInfo> Envelopes { get; } = new ObservableCollection<MailInfo>();
        public IList<MailBody> MailBodies { get; } = new List<MailBody>();

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
            get { return port.ToString(); }
            set
            {
                if (!Regex.IsMatch(value, "^\\d+$"))
                    return;

                port = int.Parse(value);
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
            mailService = new MailService();        // Normally I would use Dependency Injection / Ioc, but it's not compatible with .Net 4.5.2
            mailService.Register(this);
        }

        public void UnregisterFromMailService()
        {
            mailService.Unregister(this);
        }

        private void Start(object obj)
        {
            Envelopes.Clear();
            MailBodies.Clear();

            mailService.Connect(Servername, port, Username, Password, SelectedConnection, SelectedEncryption);
        }

        private bool CanStart(object obj)
        {
            return !string.IsNullOrEmpty(Password) &&
                    !string.IsNullOrEmpty(Username) &&
                    !string.IsNullOrEmpty(Servername) &&
                    !string.IsNullOrEmpty(Port);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        async void IMailObserver.NewMailInfoAdded(MailInfo info)
        {
            await App.Current.Dispatcher.InvokeAsync(() => Envelopes.Add(info));
        }

        void IMailObserver.NewMailBodyDownloaded(MailBody body)
        {
            MailBodies.Add(body);
        }
    }
}
