using System.Collections.Generic;

namespace JiraApi
{
    public class CommentsInfo
    {
        public int StartAt { get; set; }

        public int MaxResults { get; set; }

        public string Total { get; set; }

        public List<CommentInfo> Comments { get; set; }
    }
}