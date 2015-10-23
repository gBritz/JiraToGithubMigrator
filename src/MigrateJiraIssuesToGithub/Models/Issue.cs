﻿using System;
using System.Collections.Generic;

namespace MigrateJiraIssuesToGithub.Models
{
    public class Issue
    {
        public String JiraKey { get; set; }

        public String Title { get; set; }

        public String Content { get; set; }

        public Author Assigneed { get; set; }

        public Author Creator { get; set; }

        public DateTime? ClosedAt { get; set; }

        public String SprintName { get; set; }

        public List<Comment> Comments { get; set; }

        public List<Byte[]> FileContents { get; set; }
    }
}