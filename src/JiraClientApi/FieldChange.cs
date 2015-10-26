using System;

namespace JiraApi
{
    public class FieldChange
    {
        public String Field { get; set; }

        public String FieldType { get; set; }

        public String From { get; set; }

        public String FromString { get; set; }

        public String To { get; set; }

        public String ToString { get; set; }
    }
}