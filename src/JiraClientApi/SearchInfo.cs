using System.Collections.Generic;

namespace JiraApi
{
    public class SearchInfo
    {
        public string Expand { get; set; }

        public int StartAt { get; set; }

        public int MaxResults { get; set; }

        public int Total { get; set; }

        public List<IssueInfo> Issues { get; set; }
    }
}