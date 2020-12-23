using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
   public enum ConnectionType
    {
        IMAP,
        POP3
    }

    public enum EncryptionType
    {
        UNENCRYPTED,
        SSL_TLS,
        STARTTLS,
    }
}
