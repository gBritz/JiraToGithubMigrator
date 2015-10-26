using Flurl;
using Flurl.Http;
using System.IO;
using System.Threading.Tasks;

namespace JiraApi
{
    /// <summary>
    /// https://docs.atlassian.com/jira/REST/latest/#d2e4254
    /// </summary>
    public class JiraClientApi
    {
        private static readonly string baseUrl = "https://eljira.atlassian.net/rest/api/2";

        private readonly string username;
        private readonly string password;

        public JiraClientApi(string username, string password)
        {
            Checker.IsNull(username, "username");
            Checker.IsNull(password, "password");

            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// True to retrieve all issue history.
        /// </summary>
        public bool ViewChangelog { get; set; }

        public IssueInfo GetIssue(string key)
        {
            Checker.IsNull(key, "key");

            var issuesTask = Task.Run(async () => await GetIssueAsync(key));
            return issuesTask.Result;
        }

        public async Task<IssueInfo> GetIssueAsync(string key)
        {
            Checker.IsNull(key, "key");

            var client = baseUrl.AppendPathSegments("issue", key);

            if (ViewChangelog)
            {
                client = client.SetQueryParam("expand", "changelog");
            }

            return await client
                .WithBasicAuth(username, password)
                .GetJsonAsync<IssueInfo>();
        }

        public SearchInfo Search(string jql, int startAt, int maxResults, params string[] fields)
        {
            Checker.IsNull(jql, "jql");

            var issuesTask = Task.Run(async () => await SearchAsync(jql, startAt, maxResults, fields));
            return issuesTask.Result;
        }

        public async Task<SearchInfo> SearchAsync(string jql, int startAt, int maxResults, params string[] fields)
        {
            Checker.IsNull(jql, "jql");

            return await baseUrl
                .AppendPathSegment("search")
                .SetQueryParams(new
                {
                    jql,
                    fields = string.Join(",", fields ?? new string[0]),
                    startAt,
                    maxResults
                })
                .WithBasicAuth(username, password)
                .GetJsonAsync<SearchInfo>();
        }

        public byte[] Download(string address)
        {
            Checker.IsNull(address, "address");

            var issuesTask = Task.Run(async () => await DownloadAsync(address));
            return issuesTask.Result;
        }

        public async Task<byte[]> DownloadAsync(string address)
        {
            Checker.IsNull(address, "address");

            var stream = await address
                .WithBasicAuth(username, password)
                .GetStreamAsync();

            return ExtractOf(stream);
        }

        private static byte[] ExtractOf(Stream source)
        {
            using (var memory = new MemoryStream())
            {
                source.CopyTo(memory);
                return memory.ToArray();
            }
        }
    }
}