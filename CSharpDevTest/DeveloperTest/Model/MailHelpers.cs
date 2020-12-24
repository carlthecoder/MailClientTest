using Limilabs.Client.IMAP;
using Limilabs.Mail.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeveloperTest.Model
{
    public static class MailHelpers
    {
        public static MailInfo ComposeMailInfo(MessageInfo info)
        {
            var fromString = GetSendersAddresses(info.Envelope.From);

            return new MailInfo(info.UID.Value.ToString(), fromString.ToString(), info.Envelope.Date, info.Envelope.Subject);
        }

        public static MailInfo ComposeMailInfo(string uid, IList<MailBox> from, DateTime? date, string subject)
        {
            var fromString = GetSendersAddresses(from);            

            return new MailInfo(uid, fromString.ToString(), date, subject);
        }

        private static string GetSendersAddresses(IList<MailBox> from)
        {
            var fromString = new StringBuilder();
            foreach (var sender in from)
            {
                fromString.Append($"{sender.Address}  ");
            }
            return fromString.ToString();
        }
    }
}
