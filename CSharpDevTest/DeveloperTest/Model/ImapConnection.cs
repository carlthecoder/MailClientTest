using Limilabs.Client.IMAP;
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
    public sealed class ImapConnection : AbstractConnection, IMailConnection
    {
        private readonly object threadLocker = new object();
        private readonly ConnectionDetails connectionDetails;

        public ImapConnection(ConnectionDetails connectionDetails)
        {
            this.connectionDetails = connectionDetails;
        }

        public void DownloadMailInfo()
        {
            using (var imap = ConnectToServer(connectionDetails))
            {
                if (imap != null)
                {
                    var uids = imap.Search(Flag.All);

                    try
                    {
                        foreach (long uid in uids)
                        {
                            var info = imap.GetMessageInfoByUID(uid);

                            if (info != null)
                            {
                                var mailInfo = MailHelpers.ComposeMailInfo(info);
                                NotifyObserversMailInfoAdded(mailInfo);

                                Task.Factory.StartNew(() => DownloadMailBody(mailInfo), TaskCreationOptions.LongRunning);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"Message: {exception.Message}");
                    }
                    imap.Close();
                }
            }
        }

        private Imap ConnectToServer(ConnectionDetails connectionDetails)
        {
            var imap = new Imap();
            // Handle encryption
            //imap.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

            try
            {
                switch (connectionDetails.EncryptionType)
                {
                    case EncryptionType.UNENCRYPTED:
                        imap.Connect(connectionDetails.Servername, connectionDetails.Port);
                        break;
                    case EncryptionType.SSL_TLS:
                        imap.ConnectSSL(connectionDetails.Servername, connectionDetails.Port);  // or ConnectSSL for SSL
                        break;
                    case EncryptionType.STARTTLS:
                        imap.StartTLS();
                        imap.ConnectSSL(connectionDetails.Servername, connectionDetails.Port);
                        break;
                }

                imap.UseBestLogin(connectionDetails.Username, connectionDetails.Password);
                imap.SelectInbox();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error connecting to Server: {exception.Message}");
                imap.Close();
                imap.Dispose();
                imap = null;
            }

            return imap;
        }

        private void DownloadMailBody(MailInfo info)
        {
            if (info.isBodyDownloaded)
                return;

            using (var imap = ConnectToServer(connectionDetails))
            {
                if (imap != null)
                {
                    if (!info.isBodyDownloaded)
                    {
                        lock (threadLocker)
                        {
                            if (!info.isBodyDownloaded)
                            {
                                var uid = long.Parse(info.Uid);
                                var bodyStructure = imap.GetBodyStructureByUID(uid);
                                var text = string.Empty;

                                if (bodyStructure.Text != null)
                                {
                                    text = imap.GetTextByUID(bodyStructure.Text);
                                }

                                info.isBodyDownloaded = true;

                                var mailBody = new MailBody(info.Uid, text);
                                NotifyObserversMailBodyAdded(mailBody);
                            }
                        }
                    }

                    imap.Close();
                }
            }
        }
    }
}
