using System;

namespace JiraApi
{
    public class AttachmentInfo
    {
        public string Id { get; set; }

        public string Self { get; set; }

        public string Filename { get; set; }

        public AuthorInfo Author { get; set; }

        public DateTime Created { get; set; }

        public long Size { get; set; }

        public string MimeType { get; set; }

        public string Content { get; set; }

        public string Thumbnail { get; set; }
    }
}