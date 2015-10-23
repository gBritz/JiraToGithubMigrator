using System;

namespace MigrateJiraIssuesToGithub.Models
{
    public class Comment
    {
        public Author Creator { get; set; }

        public String Body { get; set; }
    }
}