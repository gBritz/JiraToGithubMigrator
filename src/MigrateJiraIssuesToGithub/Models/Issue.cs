﻿using System;
using System.Collections.Generic;

namespace MigrateJiraIssuesToGithub.Models
{
    public class Issue
    {
        public String JiraKey { get; set; }

        public String Title { get; set; }

        public String Content { get; set; }

        public Author Assigner { get; set; }

        public Author Assigned { get; set; }

        public DateTime? AssignedAt { get; set; }

        public Author Resolved { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public Author Creator { get; set; }

        public DateTime? CreateAt { get; set; }

        public Author InProgress { get; set; }

        public DateTime? InProgressAt { get; set; }

        public Author Closer { get; set; }

        public DateTime? ClosedAt { get; set; }

        public String SprintName { get; set; }

        public List<Comment> Comments { get; set; }

        public List<AttachmentFile> Files { get; set; }

        public List<String> Labels { get; set; }
    }
}