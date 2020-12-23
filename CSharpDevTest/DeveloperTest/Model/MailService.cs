using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class MailService : IMailService
    {
        private List<IMailObserver> observers = new List<IMailObserver>();

        public void Register(IMailObserver observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void Unregister(IMailObserver observer)
        {
            if (observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public async void Connect(string servername, int port, string username, string password, ConnectionType connection, EncryptionType encryption)
        {
            await ConnectAsync(servername, port, username, password, connection, encryption);
        }

        private async Task ConnectAsync(string servername, int port, string username, string password, ConnectionType connection, EncryptionType encryption)
        {
            await Task.Run(async () =>
            {
                using (var imap = new Imap())
                {
                    imap.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

                    try
                    {
                        imap.ConnectSSL(servername, port);  // or ConnectSSL for SSL
                        imap.UseBestLogin(username, password);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"Message: {exception.Message}");
                        return;
                    }

                    StartInfoDownload(imap);
                }
            }).ConfigureAwait(false);
        }

        private void StartInfoDownload(Imap imap)
        {
            imap.SelectInbox();
            List<long> uids = imap.Search(Flag.All);

            List<MessageInfo> messageInfos = new List<MessageInfo>();

            foreach (long uid in uids)
            {
                var info = imap.GetMessageInfoByUID(uid);

                HandleMessageInfo(info);
                messageInfos.Add(info);
            }
        }

        private void HandleMessageInfo(MessageInfo info)
        {
            var fromString = new StringBuilder();

            foreach (var sender in info.Envelope.From)
            {
                fromString.Append($"{sender.Address}, ");
            }

            var date = info.Envelope.Date;
            var subject = info.Envelope.Subject;
            var mailInfo = new MailInfo(fromString.ToString(), date, subject);
            NotifyObsersversInfoAdded(mailInfo);
        }

        private void NotifyObsersversInfoAdded(MailInfo info)
        {
            foreach (var observer in observers)
            {
                observer.NewMailInfoAdded(info);
            }
        }
    }
}
