using JiraApi;
using MigrateJiraIssuesToGithub.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MigrateJiraIssuesToGithub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var path = @"C:\Users\Guilherme\Desktop\jira-to-github";

            var jiraClientApi = new JiraClientApi("username", "password");

            Console.WriteLine("Searching issues from project ***");
            var searchResult = jiraClientApi.Search("project = *** ORDER BY created DESC", 0, 50, "customfield_10006", "labels"); //"issueType"
            Console.WriteLine("Searched issues " + searchResult.Total.ToString());

            var projectUtil = new ProjectHelper();

            var projectDetail = projectUtil.ConvertToProjectDetail(searchResult);

            var issues = new List<Issue>();

            foreach (var issueKey in projectDetail.IssueKeys)
            {
                var issueJira = jiraClientApi.GetIssue(issueKey);

                var issue = projectUtil.ConvertToIssue(issueJira);
                issue.Comments = projectUtil.ConvertToComments(issueJira.Fields);
                issue.FileContents = projectUtil.ConvertToDataFile(issueJira.Fields, jiraClientApi);

                issues.Add(issue);
            }

            projectDetail.Issues = issues;

            File.WriteAllText(@"C:\Users\Guilherme\Desktop\jira-to-github\project.json", projectDetail.ToJson());
        }
    }
}