namespace CCMS3.Helpers
{
    public class Mailrequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromEmail { get; set; }
        public List<FileAttachment> FileAttachments { get; set; } = new();
    }
}
