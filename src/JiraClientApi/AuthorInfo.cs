using System.Collections.Generic;

namespace JiraApi
{
    public class AuthorInfo
    {
        public string Self { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }

        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }

        public bool Active { get; set; }

        public string TimeZone { get; set; }

        public IDictionary<string, string> AvatarUrls { get; set; }
    }
}