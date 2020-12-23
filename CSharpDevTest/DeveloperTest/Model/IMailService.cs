using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public interface IMailService
    {
        void Register(IMailObserver observer);
        void Unregister(IMailObserver observer);

        void Connect(string servername, int port, string username, string password, ConnectionType connection, EncryptionType encryption);
    }
}
