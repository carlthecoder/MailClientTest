using Limilabs.Client.IMAP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class MailService : IMailService, IConnectionObserver
    {
        private IMailConnection connection;

        public ObservableCollection<MailInfo> MailInfos { get; private set; } = new ObservableCollection<MailInfo>();
        public IList<MailBody> MailBodies { get; private set; } = new List<MailBody>();

        public void Connect(ConnectionDetails connectionDetails)
        {
            ClearCaches();
            switch (connectionDetails.ConnectionType)
            {
                case ConnectionType.IMAP:
                    connection = new ImapConnection(connectionDetails);
                    break;
                case ConnectionType.POP3:
                    connection = new Pop3Connection(connectionDetails);
                    break;
                default:
                    return;
            }

            connection.Register(this);

            Task.Factory.StartNew(connection.DownloadMailInfo, TaskCreationOptions.LongRunning);
        }


        // Todo: solving this without an observer?
        async void IConnectionObserver.NewInfoAdded(MailInfo info)
        {
            await App.Current.Dispatcher.InvokeAsync(() => MailInfos.Add(info));
        }

        async void IConnectionObserver.NewBodyAdded(MailBody body)
        {
            await App.Current.Dispatcher.InvokeAsync(() => MailBodies.Add(body));
        }

        private void ClearCaches()
        {
            MailBodies.Clear();
            MailInfos.Clear();
        }
    }
}
