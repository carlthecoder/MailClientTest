using System.Threading;

namespace DeveloperTest.Model
{
    public interface IMailConnection
    {
        void Register(IConnectionObserver observer);
        void Unregister(IConnectionObserver observer);

        void DownloadMailInfo(CancellationToken token);
        void DownloadMailBody(MailInfo info, CancellationToken token);
    }
}
