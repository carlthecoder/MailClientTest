using DeveloperTest.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeveloperTest.ViewModel
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private const int DEFAULT_IMAP_PORT = 993;

        private readonly IMailService mailService;

        private string servername = "imap.gmail.com";
        private int port = DEFAULT_IMAP_PORT;
        private string username = "carl.claessens@gmail.com";
        private string password = "Gm@ilpas13";
        private ConnectionType selectedConnection = ConnectionType.IMAP;
        private EncryptionType selectedEncryption = EncryptionType.SSL_TLS;
        private MailInfo selectedMail;
        private string mailText;
        private bool isStartButtonVisible;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MailInfo> MailInfos { get; }

        public IList<MailBody> MailBodies { get; }

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

        public MailInfo SelectedMail
        {
            get => selectedMail;
            set
            {
                if (value == null || value == selectedMail)
                    return;

                selectedMail = value;
                LoadMailText(selectedMail);
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
            get => username;
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
            get => password;
            set
            {
                if (value == password)
                    return;

                password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartCommand));
            }
        }

        public string MailText
        {
            get => mailText;
            set
            {
                if (value == mailText)
                    return;

                mailText = value;
                OnPropertyChanged();
            }
        }

        public bool IsStartButtonVisible
        {
            get { return isStartButtonVisible; }
            private set
            {
                if (value == IsStartButtonVisible)
                    return;
                isStartButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartCommand => new RelayCommand(Start, CanStart);

        public ICommand StopCommand => new RelayCommand(Stop, _ => true);

        private ConnectionDetails ConnectionDetails => new ConnectionDetails(SelectedConnection, SelectedEncryption, Servername, port, Username, Password);


        private TaskScheduler orderScheduler;

        public MainWindowViewModel()
        {
            // Normally I would use Dependency Injection / Ioc, but it's not compatible with .Net 4.5.2
            orderScheduler = new TaskOrderScheduler();
            mailService = new MailService(orderScheduler);
            MailInfos = mailService.MailInfos;
            MailBodies = mailService.MailBodies;
            IsStartButtonVisible = true;
        }

        private void Start(object obj)
        {
            mailService.GetAllMail(ConnectionDetails);
            IsStartButtonVisible = false;
        }

        private bool CanStart(object obj)
        {
            return !string.IsNullOrEmpty(Password) &&
                   !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Servername) &&
                   !string.IsNullOrEmpty(Port);
        }

        private void Stop(object obj)
        {
            MailText = String.Empty;
            mailService.CancelOperation();
            IsStartButtonVisible = true;
        }

        private void LoadMailText(MailInfo envelope)
        {
            MailText = "";

            var body = MailBodies.FirstOrDefault(x => x.Uid == envelope.Uid);
            if (body == null)
            {
                // Todo find a way to prioritize this Task
                Task.Factory.StartNew(() => mailService.GetMailForInfo(envelope), CancellationToken.None, TaskCreationOptions.None, orderScheduler)
                    .ContinueWith(_ =>
                    {
                        body = MailBodies.FirstOrDefault(x => x.Uid == envelope.Uid);
                        MailText = body?.Text;
                    });
            }
            else
            {
                MailText = body.Text;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
