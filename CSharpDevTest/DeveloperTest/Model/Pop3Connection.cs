using Limilabs.Client.POP3;
using Limilabs.Mail;
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
    public class Pop3Connection : ConnectionBase
    {
        public Pop3Connection(ConnectionDetails details, TaskScheduler scheduler)
            : base(scheduler)
        {
            connectionDetails = details;
        }

        public override void DownloadMailInfo(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

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
                            if (token.IsCancellationRequested)
                                token.ThrowIfCancellationRequested();

                            var header = builder.CreateFromEml(pop3.GetHeadersByUID(uid));
                            var mailInfo = MailHelpers.ComposeMailInfo(uid, header.From, header.Date, header.Subject);
                            NotifyObserversMailInfoAdded(mailInfo, token);

                            Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    DownloadMailBody(mailInfo, token);
                                }
                                catch (Exception exception)
                                {
                                    Debug.WriteLine($"Body download cancelled - {exception}");
                                }
                            }, token, TaskCreationOptions.LongRunning, scheduler);
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

        public override void DownloadMailBody(MailInfo info, CancellationToken token)
        {
            if (info.isBodyDownloaded)
                return;

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            using (var pop3 = ConnectToServer(connectionDetails))
            {
                if (pop3 == null || info.isBodyDownloaded)
                    return;

                lock (threadLocker)
                {
                    if (info.isBodyDownloaded)
                        return;
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();

                    var builder = new MailBuilder();
                    var email = builder.CreateFromEml(pop3.GetMessageByUID(info.Uid.ToString()));
                    var html = email.GetBodyAsHtml();
                    var text = email.GetTextFromHtml();
                    var mailBody = new MailBody(info.Uid, text, html);

                    NotifyObserversMailBodyAdded(mailBody, token);
                    info.isBodyDownloaded = true;
                }

                pop3.Close();
            }

            return;
        }

        private Pop3 ConnectToServer(ConnectionDetails connectionDetails)
        {
            var pop3 = GetConnectionClient(connectionDetails) as Pop3;
            if (pop3 == null)
                return null;

            try
            {
                pop3.Login(connectionDetails.Username, connectionDetails.Password);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error connecting to Server: {exception.Message}");
                pop3.Dispose();
                pop3 = null;
            }

            return pop3;
        }
    }
}
