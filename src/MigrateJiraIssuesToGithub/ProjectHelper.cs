﻿using JiraApi;
using MigrateJiraIssuesToGithub.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MigrateJiraIssuesToGithub
{
    public class ProjectHelper
    {
        public ProjectDetail ConvertToProjectDetail(SearchInfo searchResult)
        {
            Checker.IsNull(searchResult, "searchResult");

            var result = new ProjectDetail();

            foreach (var issueJira in searchResult.Issues)
            {
                var issueFields = issueJira.Fields;

                AddIssueTypeAsLabelIfNoContains(issueFields, result.Labels);
                AddSprintIfNoContains(issueFields, result.Sprints);
                AddLabelIfNoContains(issueFields, result.Labels);

                result.IssueKeys.Add(issueJira.Key);
            }

            return result;
        }

        public Issue ConvertToIssue(IssueInfo issueJira)
        {
            Checker.IsNull(issueJira, "issueJira");

            var issueFields = issueJira.Fields;

            var issue = new Issue
            {
                JiraKey = issueJira.Key,
                Title = issueFields.Summary,
                Content = issueFields.Description,
                ClosedAt = issueFields.Status.IsClosed ? (DateTime?)issueFields.Updated : null,
                Creator = new Author
                {
                    Name = issueFields.Reporter.DisplayName,
                    Email = issueFields.Reporter.EmailAddress
                }
            };

            if (HasSprintDescription(issueFields))
            {
                var sprint = SprintUtil.Parse(issueFields.customfield_10006[0]);
                issue.SprintName = sprint.Name;
            }

            if (issueFields.Assignee != null)
            {
                issue.Assigneed = new Author
                {
                    Name = issueFields.Assignee.DisplayName,
                    Email = issueFields.Assignee.EmailAddress
                };
            }

            issue.Comments = ConvertToComments(issueFields);

            return issue;
        }

        public List<Comment> ConvertToComments(FieldInfo issueFields)
        {
            Checker.IsNull(issueFields, "issueFields");

            var result = new List<Comment>();

            if (issueFields.Comment != null && issueFields.Comment.Comments != null)
            {
                foreach (var comment in issueFields.Comment.Comments)
                {
                    result.Add(new Comment
                    {
                        Body = comment.Body,
                        Creator = new Author
                        {
                            Name = comment.Author.DisplayName,
                            Email = comment.Author.EmailAddress
                        }
                    });
                }
            }

            return result;
        }

        public List<byte[]> ConvertToDataFile(FieldInfo issueFields, JiraClientApi jiraClientApi)
        {
            Checker.IsNull(issueFields, "issueFields");
            Checker.IsNull(jiraClientApi, "jiraClientApi");

            var result = new List<byte[]>();

            if (issueFields.Attachment != null && issueFields.Attachment.Length > 0)
            {
                foreach (var attachment in issueFields.Attachment)
                {
                    result.Add(jiraClientApi.Download(attachment.Content));
                }
            }

            return result;
        }

        public string ReplaceJiraMentionsToGithub(string text)
        {
            return Regex.Replace(text, @"(\[\~)(\w+)(\])", "@$2");
        }

        private void AddSprintIfNoContains(FieldInfo issueFields, List<string> sprints)
        {
            if (HasSprintDescription(issueFields))
            {
                var sprint = SprintUtil.Parse(issueFields.customfield_10006[0]);
                if (!sprints.Contains(sprint.Name))
                {
                    sprints.Add(sprint.Name);
                }
            }
        }

        private bool HasSprintDescription(FieldInfo issueFields)
        {
            var field10006 = issueFields.customfield_10006;
            return field10006 != null && field10006.Count > 0 && !String.IsNullOrEmpty(field10006[0]);
        }

        private void AddIssueTypeAsLabelIfNoContains(FieldInfo issueFields, List<string> labels)
        {
            if (!labels.Contains(issueFields.IssueType.Name))
            {
                labels.Add(issueFields.IssueType.Name);
            }
        }

        private void AddLabelIfNoContains(FieldInfo issueFields, List<string> labels)
        {
            foreach (var lb in issueFields.Labels)
            {
                if (!labels.Contains(lb))
                {
                    labels.Add(lb);
                }
            }
        }
    }
}