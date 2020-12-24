using Limilabs.Client.POP3;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public class Pop3Connection : AbstractConnection, IMailConnection
    {
        private readonly object threadLocker = new object();
        private readonly ConnectionDetails connectionDetails;

        public Pop3Connection(ConnectionDetails connectionDetails)
        {
            this.connectionDetails = connectionDetails;
        }

        public void DownloadMailInfo()
        {
            using (var pop3 = ConnectToServer(connectionDetails))
            {
                if (pop3 != null)
                {
                    var uids = pop3.GetAll();

                    try
                    {
                        var builder = new MailBuilder();
                        foreach (var uid in uids)
                        {
                            var header = builder.CreateFromEml(pop3.GetHeadersByUID(uid));
                            var mailInfo = MailHelpers.ComposeMailInfo(uid, header.From, header.Date, header.Subject);
                            NotifyObserversMailInfoAdded(mailInfo);

                            Task.Factory.StartNew(() => DownloadMailBody(mailInfo), TaskCreationOptions.LongRunning);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"Message: {exception.Message}");
                    }
                    pop3.Close();
                }
            }
        }


        private Pop3 ConnectToServer(ConnectionDetails connectionDetails)
        {
            var pop3 = new Pop3();

            // Handle encryption
            //pop3.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

            try
            {
                switch (connectionDetails.EncryptionType)
                {
                    case EncryptionType.UNENCRYPTED:
                        pop3.Connect(connectionDetails.Servername, connectionDetails.Port);
                        break;
                    case EncryptionType.SSL_TLS:
                        pop3.ConnectSSL(connectionDetails.Servername, connectionDetails.Port);
                        break;
                    case EncryptionType.STARTTLS:
                        pop3.StartTLS();
                        pop3.ConnectSSL(connectionDetails.Servername, connectionDetails.Port);
                        break;
                }

                pop3.Login(connectionDetails.Username, connectionDetails.Password);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error connecting to Server: {exception.Message}");
                pop3.Close();
                pop3.Dispose();
                pop3 = null;
            }

            return pop3;
        }


        private void DownloadMailBody(MailInfo info)
        {
            if (info.isBodyDownloaded)
                return;

            using (var pop3 = ConnectToServer(connectionDetails))
            {
                if (pop3 != null)
                {
                    if (!info.isBodyDownloaded)
                    {
                        lock (threadLocker)
                        {
                            if (!info.isBodyDownloaded)
                            {
                                var builder = new MailBuilder();
                                var email = builder.CreateFromEml(pop3.GetMessageByUID(info.Uid.ToString()));

                                var text = email.Text;
                                info.isBodyDownloaded = true;
                                var mailBody = new MailBody(info.Uid, text);
                                NotifyObserversMailBodyAdded(mailBody);
                            }
                        }
                    }
                    pop3.Close();
                }
            }
        }


    }
}
