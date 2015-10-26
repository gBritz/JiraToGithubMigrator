using System;

namespace MigrateJiraIssuesToGithub.Models
{
    public class AttachmentFile
    {
        public String FileName { get; set; }

        public String MimeType { get; set; }

        public Byte[] Content { get; set; }
    }
}