using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class MailService : IMailService
    {
        // Todo: input correct algorithm (maybe a strategy pattern here..)
        public async Task ConnectAsync(string servername, int port, string username, string password)
        {
            await Task.Run(() =>
            {
                using (var imap = new Imap())
                {
                    imap.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                    imap.ConnectSSL(servername, port);  // or ConnectSSL for SSL
                    imap.UseBestLogin("carl.claessens@gmail.com", "Gm@ilpas13");
                    imap.SelectInbox();
                    var folders = imap.GetFolders();
                    List<long> uids = imap.Search(Flag.All);
                    foreach (long uid in uids)
                    {
                        IMail email = new MailBuilder()
                            .CreateFromEml(imap.GetMessageByUID(uid));
                        Console.WriteLine(email.Subject);
                    }
                    imap.Close();
                }
            }).ConfigureAwait(false);
        }
    }
}
