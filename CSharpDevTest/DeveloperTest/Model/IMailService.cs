using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public interface IMailService
    {
        Task ConnectAsync(string servername, int port, string username, string password);

    }
}
