using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public interface IMailObserver
    {
        void NewMailInfoAdded(MailInfo info);

        void NewMailBodyDownloaded(MailBody body);
    }
}
