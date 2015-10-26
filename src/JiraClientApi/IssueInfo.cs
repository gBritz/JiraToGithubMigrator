using System;

namespace JiraApi
{
    public class IssueInfo
    {
        public Int32 Id { get; set; }

        public String Key { get; set; }

        public String Expand { get; set; }

        public String Self { get; set; }

        public FieldInfo Fields { get; set; }

        public IssueChangelog Changelog { get; set; }
    }
}