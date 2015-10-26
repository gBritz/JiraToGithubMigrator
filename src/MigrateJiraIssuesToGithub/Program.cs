using JiraApi;
using MigrateJiraIssuesToGithub.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MigrateJiraIssuesToGithub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var path = @"C:\Users\Guilherme\Desktop\jira-to-github";

            var jiraClientApi = new JiraClientApi("username", "password");
            var markdownConverter = new MarkdownFlavorConverter();
            var projectUtil = new ProjectHelper(markdownConverter);
            var currentPage = 0;
            var countIssues = 50;

            Log("INFO: searching project description in jira issues");
            ProjectDetail projectDetail = RetrieveProjectDescription("***", currentPage, countIssues, jiraClientApi, projectUtil);

            while (currentPage < projectDetail.TotalIssues)
            {
                currentPage += countIssues;
                Log(String.Format("INFO: retrieve currentPage={0}, countIssues={1}, total={2}", currentPage, countIssues, projectDetail.TotalIssues));
                var projectResult = RetrieveProjectDescription("***", currentPage, countIssues, jiraClientApi, projectUtil);
                projectDetail = MergeProjects(projectDetail, projectResult);
            }

            File.WriteAllText(@"C:\Users\Guilherme\Desktop\jira-to-github\project.json", projectDetail.ToJson());

            Log("INFO: success!");
            Log("Press any key to exit.");
            Console.ReadKey();
        }

        public static ProjectDetail RetrieveProjectDescription(string projectName, int startAt, int countIssues, JiraClientApi clientApi, ProjectHelper projectUtil)
        {
            var jql = String.Format("project = {0} ORDER BY created ASC", projectName);
            Log("Searching issues from project " + projectName);
            var searchResult = clientApi.Search(jql, startAt, countIssues, "customfield_10006", "issuetype", "labels");
            Log("Searched issues " + searchResult.Total.ToString());
            return projectUtil.ConvertToProjectDetail(searchResult);
        }

        public static ProjectDetail MergeProjects(ProjectDetail projectLeft, ProjectDetail projectRight)
        {
            var result = new ProjectDetail();

            result.Labels.AddRange(projectLeft.Labels.Union(projectRight.Labels).Distinct());
            result.IssueKeys.AddRange(projectLeft.IssueKeys.Union(projectRight.IssueKeys).Distinct());
            result.Sprints.AddRange(projectLeft.Sprints.Union(projectRight.Sprints).Distinct());
            result.TotalIssues = projectLeft.TotalIssues;

            return result;
        }

        public static List<Issue> RetrieveIssues(List<String> issueKeys, JiraClientApi clientApi, ProjectHelper projectUtil)
        {
            var issues = new List<Issue>();

            foreach (var issueKey in issueKeys)
            {
                var issueJira = clientApi.GetIssue(issueKey);

                var issue = projectUtil.ConvertToIssue(issueJira);
                issue.Comments = projectUtil.ConvertToComments(issueJira.Fields);
                issue.Files = projectUtil.ConvertToDataFile(issueJira.Fields, clientApi);

                issues.Add(issue);
            }

            return issues;
        }

        public static void Log(String msg)
        {
            Console.WriteLine(String.Format("{0:dd/MM/yyyy hh:mm:ss.fff} - {1}", DateTime.Now, msg));
        }
    }
}