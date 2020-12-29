using Limilabs.Client.IMAP;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class ImapConnection : ConnectionBase
    {
        public ImapConnection(ConnectionDetails details)
        {
            connectionDetails = details;
        }

        public override void DownloadMailInfo(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            using (var imap = ConnectToServer(connectionDetails))
            {
                if (imap != null)
                {
                    var uids = imap.Search(Flag.All);

                    try
                    {
                        foreach (long uid in uids)
                        {
                            if (token.IsCancellationRequested)
                                token.ThrowIfCancellationRequested();

                            var info = imap.GetMessageInfoByUID(uid);
                            if (info != null)
                            {
                                var mailInfo = MailHelpers.ComposeMailInfo(info);
                                NotifyObserversMailInfoAdded(mailInfo, token);

                                Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        DownloadMailBody(mailInfo, token);
                                    }
                                    catch (Exception)
                                    {
                                        Debug.WriteLine("Body download cancelled.");
                                    }
                                }, TaskCreationOptions.LongRunning);
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

        public override void DownloadMailBody(MailInfo info, CancellationToken token)
        {
            if (info.isBodyDownloaded)
                return;

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

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
                                if (token.IsCancellationRequested)
                                    token.ThrowIfCancellationRequested();

                                var uid = long.Parse(info.Uid);
                                var bodyStructure = imap.GetBodyStructureByUID(uid);
                                var text = string.Empty;
                                var html = string.Empty;

                                if (bodyStructure.Text != null)
                                {
                                    text = imap.GetTextByUID(bodyStructure.Text);
                                }

                                if (bodyStructure.Html != null)
                                {
                                    html = imap.GetTextByUID(bodyStructure.Html);
                                }

                                info.isBodyDownloaded = true;

                                var mailBody = new MailBody(info.Uid, text, html);
                                NotifyObserversMailBodyAdded(mailBody, token);
                            }
                        }
                    }

                    imap.Close();
                }
            }
        }

        private Imap ConnectToServer(ConnectionDetails connectionDetails)
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
