using Limilabs.Client;
using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public abstract class ConnectionBase : IMailConnection
    {
        protected readonly object threadLocker = new object();
        protected readonly IList<IConnectionObserver> observers = new List<IConnectionObserver>();

        protected ConnectionDetails connectionDetails;

        public static IMailConnection CreateConnection(ConnectionDetails details)
        {
            IMailConnection connection = null;
            switch (details.ConnectionType)
            {
                case ConnectionType.IMAP:
                    connection = new ImapConnection(details);
                    break;
                case ConnectionType.POP3:
                    connection = new Pop3Connection(details);
                    break;
            }

            return connection;
        }

        public void Register(IConnectionObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            if (observers.Contains(observer))
                observers.Remove(observer);
        }

        public abstract void DownloadMailInfo(CancellationToken token);
        public abstract void DownloadMailBody(MailInfo info, CancellationToken token);

        protected void NotifyObserversMailInfoAdded(MailInfo info, CancellationToken token)
        {
            foreach (var observer in observers)
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                observer.NewInfoAdded(info);
            }
        }

        protected void NotifyObserversMailBodyAdded(MailBody body, CancellationToken token)
        {
            foreach (var observer in observers)
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                observer.NewBodyAdded(body);
            }
        }

        protected ClientBase GetConnectionClient(ConnectionDetails details)
        {
            ClientBase client = null;
            bool isImapConnection = false;

            switch (details.ConnectionType)
            {
                case ConnectionType.IMAP:
                    client = new Imap();
                    isImapConnection = true;
                    break;

                case ConnectionType.POP3:
                    client = new Pop3();
                    break;
            }

            try
            {
                switch (details.EncryptionType)
                {
                    case EncryptionType.UNENCRYPTED:
                        client.Connect(details.Servername, details.Port);
                        break;

                    case EncryptionType.SSL_TLS:
                        client.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
                        client.ConnectSSL(details.Servername, details.Port);  // or ConnectSSL for SSL
                        break;

                    case EncryptionType.STARTTLS:
                        // Limilabs docs:
                        // Many companies (e.g. Gmail, Outlook.com) disabled plain IMAP (port 143) and plain POP3 (port 110), so people must use a SSL/TLS encrypted connection –
                        // this removes the need for having STARTTLS command completely.
                        client.Connect(details.Servername, details.Port);
                        if (isImapConnection)
                            ((Imap)client).StartTLS();
                        else
                            ((Pop3)client).StartTLS();

                        break;
                }
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine($"An excaption occurred while connecting the client: {exception}");
            }

            return client;
        }
    }
}
