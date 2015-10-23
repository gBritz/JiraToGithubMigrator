using System;
using System.Collections.Generic;

namespace MigrateJiraIssuesToGithub.Models
{
    public class Sprint
    {
        public String Title { get; set; }

        public Boolean IsClosed { get; set; }

        public DateTime? DueOn { get; set; }

        public List<Issue> Issues { get; set; }
    }
}