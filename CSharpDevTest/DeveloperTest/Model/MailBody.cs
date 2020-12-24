namespace DeveloperTest.Model
{
    public sealed class MailBody
    {
        public string Uid { get; set; }
        public string Text { get; set; }

        public MailBody(string uid, string text)
        {
            Uid = uid;
            Text = text;
        }
    }
}
