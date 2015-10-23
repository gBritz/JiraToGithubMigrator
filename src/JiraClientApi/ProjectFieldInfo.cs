using System.Collections.Generic;

namespace JiraApi
{
    public class ProjectFieldInfo
    {
        public string Self { get; set; }

        public string Id { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public IDictionary<string, string> AvatarUrls { get; set; }
    }
}