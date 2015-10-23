using System.Linq;

namespace JiraApi
{
    public class SprintUtil
    {
        public static SprintInfo Parse(string content)
        {
            var start = content.IndexOf('[');
            var end = content.LastIndexOf(']');

            var sprintInfo = content.Substring(start, end - start);
            var parts = sprintInfo.Split(',').Select(p => p.Split('=')).ToDictionary(p => p[0], p => p[1]);

            return new SprintInfo
            {
                Name = parts["name"],
                IsClosed = parts["state"] == "Closed"
            };
        }
    }
}