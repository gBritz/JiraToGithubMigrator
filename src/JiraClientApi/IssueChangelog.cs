using System;
using System.Collections.Generic;

namespace JiraApi
{
    public class IssueChangelog
    {
        public Int32 StartAt { get; set; }

        public Int32 MaxResults { get; set; }

        public Int32 Total { get; set; }

        public List<ChangelogHistory> Histories { get; set; }
    }
}