using System;
using System.Linq;
using System.Text;

namespace JiraApi
{
    public class SprintUtil
    {
        public static SprintInfo Parse(string content)
        {
            var start = content.IndexOf('[') + 1;
            var end = content.LastIndexOf(']');

            var sprintInfo = content.Substring(start, end - start);
            sprintInfo = ReplaceRegion(sprintInfo, ',', ';', sprintInfo.IndexOf(",name=")+1, sprintInfo.IndexOf(",startDate=")-1);

            var parts = sprintInfo.Split(',').Select(p => p.Split('=')).ToDictionary(p => p[0], p => p[1]);

            return new SprintInfo
            {
                Name = parts["name"].Replace(';', ','),
                IsClosed = parts["state"] == "Closed"
            };
        }

        private static String ReplaceRegion(String input, Char oldStr, Char newStr, Int32 startAt, Int32 endsAt)
        {
            var sb = new StringBuilder(input);
            sb.Replace(oldStr, newStr, startAt, endsAt - startAt);
            return sb.ToString();
        }
    }
}