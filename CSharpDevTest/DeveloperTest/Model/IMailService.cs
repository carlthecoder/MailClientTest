using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeveloperTest.Model
{
    public interface IMailService
    {
        ObservableCollection<MailInfo> MailInfos { get; }
        IList<MailBody> MailBodies { get; }

        void GetAllMail(ConnectionDetails connectionDetails);
        void GetMailForInfo(MailInfo info);
        void CancelOperation();
    }
}
