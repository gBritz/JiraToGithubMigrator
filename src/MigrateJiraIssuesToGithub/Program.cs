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
            var projectPath = @"C:\Users\Guilherme\Desktop\jira-to-github";
            var issuePath =  Path.Combine(projectPath, "issues");

            var jiraClientApi = new JiraClientApi("username", "password");
            var markdownConverter = new MarkdownFlavorConverter();
            var projectUtil = new ProjectHelper(markdownConverter);

            // var projectDetail = RetrieveProjectDescription("***", jiraClientApi, projectUtil);
            // File.WriteAllText(projectPath, projectDetail.ToJson());

            var projectFileFullPath = Path.Combine(projectPath, "project.json");
            var projectDetail = File.ReadAllText(projectFileFullPath).ToObject<ProjectDetail>();
            SaveIssuesInformation(projectDetail.IssueKeys, jiraClientApi, projectUtil, issuePath);

            Log("INFO: success!");
            Log("Press any key to exit.");
            Console.ReadKey();
        }

        public static ProjectDetail RetrieveProjectDescription(String projectName, JiraClientApi clientApi, ProjectHelper projectUtil)
        {
            var currentPage = 0;
            var countIssues = 50;

            Log("INFO: searching project description in jira issues");
            ProjectDetail projectDetail = RetrieveProjectDescriptionPagined(projectName, currentPage, countIssues, clientApi, projectUtil);

            while (currentPage < projectDetail.TotalIssues)
            {
                currentPage += countIssues;
                Log(String.Format("INFO: retrieve currentPage={0}, countIssues={1}, total={2}", currentPage, countIssues, projectDetail.TotalIssues));
                var projectResult = RetrieveProjectDescriptionPagined(projectName, currentPage, countIssues, clientApi, projectUtil);
                projectDetail = MergeProjects(projectDetail, projectResult);
            }

            return projectDetail;
        }

        public static ProjectDetail RetrieveProjectDescriptionPagined(string projectName, int startAt, int countIssues, JiraClientApi clientApi, ProjectHelper projectUtil)
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

        public static void SaveIssuesInformation(List<String> issueKeys, JiraClientApi clientApi, ProjectHelper projectUtil, String path)
        {
            Log("INFO: start save information issues");
            var savedIssue = 0;
            var index = 0;

            foreach (var issueKey in issueKeys)
            {
                var issueJira = clientApi.GetIssue(issueKey);

                var issue = projectUtil.ConvertToIssue(issueJira);
                issue.Comments = projectUtil.ConvertToComments(issueJira.Fields);
                issue.Files = projectUtil.ConvertToDataFile(issueJira.Fields, clientApi);

                var filePath = Path.Combine(path, issue.JiraKey + ".txt");
                File.WriteAllText(filePath, issue.ToJson());
                savedIssue++;

                Log(String.Format("INFO: {2} - saved {0}, spend {1} issues", issue.JiraKey, issueKeys.Count - savedIssue, index));

                index++;
            }

            Log("INFO: completed");
        }

        public static void Log(String msg)
        {
            Console.WriteLine(String.Format("{0:dd/MM/yyyy hh:mm:ss.fff} - {1}", DateTime.Now, msg));
        }
    }
}