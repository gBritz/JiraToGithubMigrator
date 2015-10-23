using System;

namespace JiraApi
{
    public class CommentInfo
    {
        public string Self { get; set; }

        public string Id { get; set; }

        public AuthorInfo Author { get; set; }

        public string Body { get; set; }

        public AuthorInfo UpdateAuthor { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}