using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public interface IMailService
    {
        ObservableCollection<MailInfo> MailInfos { get; }
        IList<MailBody> MailBodies { get; }

        void Connect(ConnectionDetails connectionDetails);
    }
}
