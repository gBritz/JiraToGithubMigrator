using System;
using System.Collections.Generic;

namespace JiraApi
{
    public class ChangelogHistory
    {
        public Int32 Id { get; set; }

        public AuthorInfo Author { get; set; }

        public DateTime Created { get; set; }

        public List<FieldChange> Items { get; set; }
    }
}