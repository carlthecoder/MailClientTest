using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
