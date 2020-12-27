using Limilabs.Client.IMAP;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class ImapConnection : ConnectionBase, IMailConnection
    {
        public ImapConnection(ConnectionDetails details)
        {
            connectionDetails = details;
        }

        public void DownloadMailInfo()
        {
            using (var imap = ConnectClientToServer(connectionDetails))
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

        public void DownloadMailBody(MailInfo info)
        {
            if (info.isBodyDownloaded)
                return;

            using (var imap = ConnectClientToServer(connectionDetails))
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

        private Imap ConnectClientToServer(ConnectionDetails connectionDetails)
        {
            var imap = GetConnectionClient(connectionDetails) as Imap;
            if (imap == null)
                return null;

            try
            {
                imap.UseBestLogin(connectionDetails.Username, connectionDetails.Password);
                imap.SelectInbox();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error connecting to Server: {exception.Message}");
                imap.Dispose();
                imap = null;
            }

            return imap;
        }
    }
}
