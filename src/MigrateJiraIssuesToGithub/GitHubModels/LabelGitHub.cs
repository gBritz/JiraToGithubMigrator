using System;

namespace MigrateJiraIssuesToGithub.GitHubModels
{
    public class LabelGitHub
    {
        public string Color { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }
    }
}