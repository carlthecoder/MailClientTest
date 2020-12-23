using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class MailService : IMailService
    {
        private readonly List<IMailObserver> observers = new List<IMailObserver>();
        private Imap imap;
        private object bodyThreadLocker = new object();

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
                imap = new Imap();
                
                imap.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

                try
                {
                    //Todo determine connection Algorithm
                    imap.ConnectSSL(servername, port);  // or ConnectSSL for SSL
                    imap.UseBestLogin(username, password);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Message: {exception.Message}");
                    return;
                }

                await StartInfoDownloadAsync(imap).ConfigureAwait(false);

                imap.Dispose();
               
            }).ConfigureAwait(false);
        }

        private async Task StartInfoDownloadAsync(Imap imap)
        {
            imap.SelectInbox();
            var uids = imap.Search(Flag.All);
            var messageInfos = new List<MessageInfo>();

            try
            {
                foreach (long uid in uids)
                {
                    var info = imap.GetMessageInfoByUID(uid);

                    if (info != null)
                    {
                        HandleMessageInfo(info, uid);
                        messageInfos.Add(info);
                    }

                    //Task.Run(() => StartBodyDownloadForUid(uid, imap)).ConfigureAwait(false);                    
                }
                
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Message: {exception.Message}");
            }
        }       

        private void HandleMessageInfo(MessageInfo info, long uid)
        {
            var fromString = new StringBuilder();
            foreach (var sender in info.Envelope.From)
            {
                fromString.Append($"{sender.Address}, ");
            }

            var date = info.Envelope.Date;
            var subject = info.Envelope.Subject;
            var mailInfo = new MailInfo(uid, fromString.ToString(), date, subject);
            NotifyObsersversInfoAdded(mailInfo);
        }

        private void NotifyObsersversInfoAdded(MailInfo info)
        {
            foreach (var observer in observers)
            {
                observer.NewMailInfoAdded(info);
            }
        }

        private void StartBodyDownloadForUid(long uid, Imap imap)
        {
            string text = "";
            string html = "";

            lock(bodyThreadLocker)
            {
                var body = imap.GetBodyStructureByUID(uid);

                if (body.Text != null)
                {
                    text = imap.GetTextByUID(body.Text);
                }

                if (body.Html != null)
                {
                    html = imap.GetTextByUID(body.Html);
                }               
            }

            var mailBody = new MailBody(uid, text, html);

            NotifyObsersversBodyDownloaded(mailBody);
        }

        private void NotifyObsersversBodyDownloaded(MailBody body)
        {
            foreach (var observer in observers)
            {
                observer.NewMailBodyDownloaded(body);
            }
        }
    }
}
