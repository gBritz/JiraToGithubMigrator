using System.Collections.Generic;

namespace JiraApi
{
    public class IssueInfo
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Expand { get; set; }

        public string Self { get; set; }

        public FieldInfo Fields { get; set; }
    }
}