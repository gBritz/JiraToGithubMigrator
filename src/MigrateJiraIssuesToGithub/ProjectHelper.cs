﻿using JiraApi;
using MigrateJiraIssuesToGithub.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MigrateJiraIssuesToGithub
{
    public class ProjectHelper
    {
        private readonly MarkdownFlavorConverter markdownConverter;

        public ProjectHelper(MarkdownFlavorConverter markdownConverter)
        {
            Checker.IsNull(markdownConverter, "markdownConverter");

            this.markdownConverter = markdownConverter;
        }

        public ProjectDetail ConvertToProjectDetail(SearchInfo searchResult)
        {
            Checker.IsNull(searchResult, "searchResult");

            var result = new ProjectDetail
            {
                TotalIssues = searchResult.Total
            };

            foreach (var issueJira in searchResult.Issues)
            {
                var issueFields = issueJira.Fields;

                AddIssueTypeAsLabelIfNoContains(issueFields, result.Labels);
                try
                {
                    AddSprintIfNoContains(issueFields, result.Sprints);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
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
                Creator = ConvertToAuthor(issueFields.Reporter),
                CreateAt = issueFields.Created,
                Labels = issueFields.Labels ?? new List<String>()
            };

            if (issueFields.Status.IsClosed)
            {
                var history = issueJira.Changelog.Histories.LastOrDefault(h => h.Items.Any(i => i.Field == "status" && i.ToString == issueFields.Status.Name));

                Debug.Assert(history == null, "history closed not found.");

                issue.Closer = ConvertToAuthor(history.Author);
                issue.ClosedAt = history.Created;
            }

            if (HasSprintDescription(issueFields))
            {
                var sprint = SprintUtil.Parse(issueFields.customfield_10006[0]);
                issue.SprintName = sprint.Name;
            }

            if (issueFields.Assignee != null)
            {
                issue.Assigned = ConvertToAuthor(issueFields.Assignee);

                var history = issueJira.Changelog.Histories.LastOrDefault(h => h.Items.Any(i => i.Field == "assignee"));
                if (history != null)
                {
                    issue.Assigner = new Author { Name = history.Items.First(i => i.Field == "assignee").FromString };
                    issue.AssignedAt = history.Created;
                }
                else
                {
                    //obs.: creator issue.
                    issue.Assigner = ConvertToAuthor(issueFields.Reporter);
                    issue.AssignedAt = issueFields.Created;
                }
            }

            var inProgressStatus = issueJira.Changelog.Histories.LastOrDefault(h => h.Items.Any(i => i.Field == "status" && i.FromString == "In Progress"));

            if (inProgressStatus != null)
            {
                issue.InProgress = ConvertToAuthor(inProgressStatus.Author);
                issue.InProgressAt = inProgressStatus.Created;
            }

            var resolvedStatus = issueJira.Changelog.Histories.LastOrDefault(h => h.Items.Any(i => i.Field == "status" && i.FromString == "Resolved"));

            if (resolvedStatus != null)
            {
                issue.InProgress = ConvertToAuthor(resolvedStatus.Author);
                issue.InProgressAt = resolvedStatus.Created;
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
                    var content = markdownConverter.ReplaceFromJiraToGithub(comment.Body);

                    result.Add(new Comment
                    {
                        Body = content,
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

        public List<AttachmentFile> ConvertToDataFile(FieldInfo issueFields, JiraClientApi jiraClientApi)
        {
            Checker.IsNull(issueFields, "issueFields");
            Checker.IsNull(jiraClientApi, "jiraClientApi");

            var result = new List<AttachmentFile>();

            if (issueFields.Attachment != null && issueFields.Attachment.Length > 0)
            {
                foreach (var attachment in issueFields.Attachment)
                {
                    var file = new AttachmentFile
                    {
                        FileName = attachment.Filename,
                        MimeType = attachment.MimeType,
                        Content = jiraClientApi.Download(attachment.Content)
                    };
                    result.Add(file);
                }
            }

            return result;
        }

        public Author ConvertToAuthor(AuthorInfo author)
        {
            Checker.IsNull(author, "author");

            return new Author
            {
                Name = author.DisplayName,
                Email = author.EmailAddress
            };
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