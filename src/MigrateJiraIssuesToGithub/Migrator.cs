using MigrateJiraIssuesToGithub.GitHubModels;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateJiraIssuesToGithub
{
    public class Migrator
    {
        private readonly String githubOrganizationName;
        private readonly String githubRepositoryName;
        private readonly ProjectHelper projectHelper;
        private readonly GitHubClient client;

        public Migrator(ProjectHelper projectHelper, GitHubClient client, String githubOrganizationName, String githubRepositoryName)
        {
            Checker.IsNull(projectHelper, "projectHelper");
            Checker.IsNull(client, "client");
            Checker.IsNull(githubOrganizationName, "githubOrganizationName");
            Checker.IsNull(githubRepositoryName, "githubRepositoryName");

            this.projectHelper = projectHelper;
            this.client = client;
            this.githubOrganizationName = githubOrganizationName;
            this.githubRepositoryName = githubRepositoryName;
        }

        public Action<String> Logger { get; set; }

        public List<Milestone> MigrateToMilestones(List<String> sprintNames)
        {
            Checker.IsNull(sprintNames, "sprintNames");

            var milestones = new List<Milestone>();

            Log("Info: Creating all milestones.");
            foreach (var sprintName in sprintNames)
            {
                var newMilestone = new NewMilestone(sprintName)
                {
                    Description = sprintName,
                    DueOn = null,
                    State = ItemState.Closed
                };
                var milestoneTask = Task.Run<Milestone>(async () => await client.Issue.Milestone.Create(githubOrganizationName, githubRepositoryName, newMilestone));
                milestones.Add(milestoneTask.Result);
            }
            Log("Info: Milestones are created with success.");

            return milestones;
        }

        public List<Label> MigrateToLabels(List<String> labelNames)
        {
            var labels = new List<Label>();

            Log("Info: Creating all labels.");
            foreach (var labelName in labelNames)
            {
                var newLabel = new NewLabel(labelName, "006b75");

                try
                {
                    var labelTask = Task.Run<Label>(async () => await client.Issue.Labels.Create(githubOrganizationName, githubRepositoryName, newLabel));
                    labels.Add(labelTask.Result);
                }
                catch (Exception ex)
                {
                    Log(String.Format("ERROR: not creating label {0}, get him.", labelName));

                    var labelTask = Task.Run<Label>(async () => await client.Issue.Labels.Get(githubOrganizationName, githubRepositoryName, labelName));
                    labels.Add(labelTask.Result);
                }
            }
            Log("Info: Labels are created with success.");

            return labels;
        }

        public void MigrateToIssue(MigrateJiraIssuesToGithub.Models.Issue jiraIssue, List<MilestoneGitHub> milestones, List<LabelGitHub> labels, ref NewIssue newIssue)
        {
            if (String.IsNullOrEmpty(jiraIssue.SprintName))
            {
                jiraIssue.SprintName = "1.0 Inicial";
            }

            Octokit.Issue issueResult = null;

            if (newIssue == null)
            {
                newIssue = new NewIssue(jiraIssue.Title)
                {
                    Body = jiraIssue.Content,
                    Milestone = milestones.First(m => m.Title == jiraIssue.SprintName).Number
                };

                foreach (var label in jiraIssue.Labels)
                {
                    newIssue.Labels.Add(label);
                }

                var nIssue = newIssue;
                var taskResult = Task.Run<Octokit.Issue>(async () => await client.Issue.Create(githubOrganizationName, githubRepositoryName, nIssue));
                issueResult = taskResult.Result;
            }

            //todo: archive...

            // obs.: atribuir usuário à tarefa
            /*if (issueYAA923.Assigned != null)
            {
                try
                {
                    client.Issue.Assignee.CheckAssignee(githubOrganizationName, githubRepositoryName, issueYAA923.Assigned.Name); //"gBritz"
                }
                catch (Exception ex)
                {
                    Log("ERROR: Não conseguiu atribuir ao usuário " + issueYAA923.Assigned.Name); //obs.: caso não exista no projeto, irá gerar erro ao cadastrar issue

                    var msg = String.Format("Originalmente atribuído para o {0} ({1}) no jira.", issueYAA923.Assigned.Name, issueYAA923.Assigned.Email);
                    client.Issue.Comment.Create(githubOrganizationName, githubRepositoryName, issueResult.Number, msg);
                }
            }*/

            var sbStatusComment = new StringBuilder();

            sbStatusComment.AppendFormat(@"
    **Status in Jira**
    {0} ({1}) `Created` at {1:dd/MM/yyyy hh:mm:ss}
", jiraIssue.Creator.Name, jiraIssue.Creator.Email, jiraIssue.CreateAt);

            if (jiraIssue.InProgress != null && jiraIssue.InProgressAt.HasValue)
            {
                sbStatusComment.AppendFormat(@"
    {0} ({1}) set `In Progress` at {2:dd/MM/yyyy hh:mm:ss}", jiraIssue.InProgress.Name, jiraIssue.InProgress.Email, jiraIssue.InProgressAt);
            }

            if (jiraIssue.Resolved != null && jiraIssue.ResolvedAt.HasValue)
            {
                sbStatusComment.AppendFormat(@"
    {0} ({1}) set `Resolved` at {2:dd/MM/yyyy hh:mm:ss}", jiraIssue.Resolved.Name, jiraIssue.Resolved.Email, jiraIssue.ResolvedAt);
            }

            if (jiraIssue.Closer != null && jiraIssue.ClosedAt.HasValue)
            {
                sbStatusComment.AppendFormat(@"
    {0} ({1}) set `Closed` at {2:dd/MM/yyyy hh:mm:ss}", jiraIssue.Closer.Name, jiraIssue.Closer.Email, jiraIssue.ClosedAt);
            }

            client.Issue.Comment.Create(githubOrganizationName, githubRepositoryName, issueResult.Number, sbStatusComment.ToString());

            // obs.: adicionar comentários
            foreach (var comment in jiraIssue.Comments)
            {
                var commentAuthor = String.Format(@"
    **Commentary** by {0} ({1})
", comment.Creator.Name, comment.Creator.Email);
                client.Issue.Comment.Create(githubOrganizationName, githubRepositoryName, issueResult.Number, commentAuthor + comment.Body);
            }

            // obs.: fechar issue
            if (jiraIssue.ClosedAt.HasValue)
            {
                var issueUpdate = new IssueUpdate
                {
                    State = ItemState.Closed
                };

                client.Issue.Update(githubOrganizationName, githubRepositoryName, issueResult.Number, issueUpdate);
            }

            Log(String.Format("Info: Issue {0} created", jiraIssue.JiraKey));
        }

        private void Log(String message)
        {
            if (Logger != null)
            {
                Logger(message);
            }
        }
    }
}