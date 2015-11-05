using Octokit;
using System;

namespace MigrateJiraIssuesToGithub.GitHubModels
{
    public class MilestoneGitHub
    {
        public String Url { get; set; }

        public Int32 Number { get; set; }

        public ItemState State { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public Int32 OpenIssues { get; set; }

        public Int32 ClosedIssues { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? DueOn { get; set; }

        public DateTimeOffset? ClosedAt { get; set; }
    }
}