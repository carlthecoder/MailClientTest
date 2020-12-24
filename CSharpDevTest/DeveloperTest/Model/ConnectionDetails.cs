using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class ConnectionDetails
    {
        public ConnectionType ConnectionType { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public string Servername { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ConnectionDetails(ConnectionType connectionType, EncryptionType encryptionType, string servername, int port, string username, string password)
        {
            ConnectionType = connectionType;
            EncryptionType = encryptionType;
            Servername = servername;
            Port = port;
            Username = username;
            Password = password;
        }
    }
}
