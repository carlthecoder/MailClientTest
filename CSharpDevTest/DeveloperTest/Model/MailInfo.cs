using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class MailInfo
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public DateTime? Date { get; set; }

        public MailInfo(string from, DateTime? date, string subject = "")
        {
            From = from;
            Subject = subject;
            Date = date;
        }
    }
}
