namespace DeveloperTest.Model
{
    public sealed class MailBody
    {
        public string Uid { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }

        public MailBody(string uid, string text, string html)
        {
            Uid = uid;
            Text = text;
            Html = html;
        }
    }
}
