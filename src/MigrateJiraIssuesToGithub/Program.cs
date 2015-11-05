using JiraApi;
using MigrateJiraIssuesToGithub.GitHubModels;
using MigrateJiraIssuesToGithub.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MigrateJiraIssuesToGithub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var githubOrganizationName = "???";
            var githubRepositoryName = "???";

            var projectPath = @"C:\Users\Guilherme\Desktop\jira-to-github";
            var issuePath =  Path.Combine(projectPath, "issues");
            var projectFileFullPath = Path.Combine(projectPath, "project.json");
            var milestonesPath = Path.Combine(projectPath, "milestones.json");
            var labelsPath = Path.Combine(projectPath, "labels.json");

            var jiraClientApi = new JiraClientApi("username", "password") { ViewChangelog = true };
            var markdownConverter = new MarkdownFlavorConverter();
            var projectUtil = new ProjectHelper(markdownConverter);
            ProjectDetail projectDetail = null;

            /*var jiraIssue = jiraClientApi.GetIssue("***");
            var issue = projectUtil.ConvertToIssue(jiraIssue);
            Console.WriteLine(issue.JiraKey);*/

            /*Log("INFO: Getting all project information...");
            projectDetail = RetrieveProjectDescription("***", jiraClientApi, projectUtil);
            Log("INFO: Saving project information!");
            File.WriteAllText(projectFileFullPath, projectDetail.ToJson());
            Log("INFO: Project information saved with success!");*/

            /*Log("INFO: Getting all issues information...");
            projectDetail = File.ReadAllText(projectFileFullPath).ToObject<ProjectDetail>();
            SaveIssuesInformation(projectDetail.IssueKeys, jiraClientApi, projectUtil, issuePath);*/

            projectDetail = File.ReadAllText(projectFileFullPath).ToObject<ProjectDetail>();

            var client = new GitHubClient(new ProductHeaderValue("jira-migrator"))
            {
                Credentials = new Credentials("username", "password")
            };

            var migrator = new Migrator(projectUtil, client, githubOrganizationName, githubRepositoryName)
            {
                Logger = Log
            };

            List<MilestoneGitHub> milestones = null;
            List<LabelGitHub> labels = null;

            if (!File.Exists(milestonesPath))
            {
                var mls = migrator.MigrateToMilestones(projectDetail.Sprints);
                File.WriteAllText(milestonesPath, mls.ToJson());
            }

            if (!File.Exists(labelsPath))
            {
                var lbs = migrator.MigrateToLabels(projectDetail.Labels);
                File.WriteAllText(labelsPath, lbs.ToJson());
            }

            milestones = File.ReadAllText(milestonesPath).ToObject<List<MilestoneGitHub>>();
            labels = File.ReadAllText(labelsPath).ToObject<List<LabelGitHub>>();

            Log("Info: Creating all issues.");
            foreach (var issueKey in projectDetail.IssueKeys)
            {
                var fileName = Path.Combine(issuePath, issueKey + ".txt");
                var jiraIssue = File.ReadAllText(fileName).ToObject<MigrateJiraIssuesToGithub.Models.Issue>();

                Octokit.Issue newIssue = null;

                SleepForMigrateIssue();

                while (!migrator.TryMigrateToIssue(jiraIssue, milestones, labels, ref newIssue))
                {
                    Log(String.Format("WARNING: issue key {0} is blocked by github.", jiraIssue.JiraKey));
                    SleepForMigrateIssue();
                }

                Log(String.Format("INFO: jira issue#{0} is created to github issue#{1}", jiraIssue.JiraKey, newIssue.Number));
            }
            Log("Info: Issues are created with success.");

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

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

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

        public static void SleepForMigrateIssue()
        {
            Log("INFO: Waiting for 30 seconds.");
            Thread.Sleep(1000 * 30); //60
        }

        public static void Log(String msg)
        {
            Console.WriteLine(String.Format("{0:dd/MM/yyyy hh:mm:ss.fff} - {1}", DateTime.Now, msg));
        }
    }
}