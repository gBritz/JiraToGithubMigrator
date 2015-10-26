using System;
using System.Collections.Generic;

namespace MigrateJiraIssuesToGithub.Models
{
    public class ProjectDetail
    {
        private readonly List<String> sprints = new List<String>();
        private readonly List<String> labels = new List<String>();
        private readonly List<String> issues = new List<String>();

        public int TotalIssues { get; set; }

        public List<String> Sprints
        {
            get { return sprints; }
        }

        public List<String> Labels
        {
            get { return labels; }
        }

        public List<String> IssueKeys
        {
            get { return issues; }
        }
    }
}