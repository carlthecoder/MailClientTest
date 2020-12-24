namespace DeveloperTest.Model
{
    public interface IConnectionObserver
    {
        void NewInfoAdded(MailInfo info);
        void NewBodyAdded(MailBody body);
    }
}