using System;
using System.Collections.Generic;

namespace JiraApi
{
    public class FieldInfo
    {
        public string Summary { get; set; }

        public string Description { get; set; }

        public int? Timespent { get; set; }

        public List<string> Labels { get; set; }

        public AuthorInfo Reporter { get; set; }

        public AuthorInfo Assignee { get; set; }

        public List<string> customfield_10006 { get; set; }

        public IssueTypeInfo IssueType { get; set; }

        public StatusInfo Status { get; set; }

        public CommentsInfo Comment { get; set; }

        public AttachmentInfo[] Attachment { get; set; }

        public ProjectFieldInfo Project { get; set; }

        //// public string[] FixVersions { get; set; }
        //// public string[] SubTasks { get; set; }

        public int? AggregateTimespent { get; set; }

        public ResolutionInfo Resolution { get; set; }

        public DateTime? ResolutionDate { get; set; }

        public long WorkRatio { get; set; }

        public DateTime? LastViewed { get; set; }

        public WatchesInfo Watchers { get; set; }

        public ProgressInfo Progress { get; set; }

        public DateTime Updated { get; set; }
    }
}