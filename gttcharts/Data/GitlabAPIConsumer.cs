﻿using GitLabApiClient;
using GitLabApiClient.Models.Notes.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gttcharts.Data
{
    public class GitlabAPIConsumer
    {
        private readonly GitlabAPIOptions options;
        private readonly TimeStringParser timeParser;

        public GitlabAPIConsumer(GitlabAPIOptions options)
        {
            this.options = options;
            this.timeParser = new TimeStringParser(options);
        }

        public async Task<(bool success, IEnumerable<Models.Issue> issues, IEnumerable<Models.Record> records)> GetData()
        {
            ConcurrentBag<Models.Issue> issues = new();
            ConcurrentBag<Models.Record> records = new();
            StyledConsoleWriter.WriteInfo($"Calling Gitlab API...");
            GitLabClient client;
            try
            {
                client = new GitLabClient(options.ApiUrl, options.Token);
            }
            catch(Exception ex)
            {
                StyledConsoleWriter.WriteError($"Error when creating GitLabAPIClient: {ex.Message}");
                return (false, null, null);
            }
            StyledConsoleWriter.WriteInfo($"Gettings issues...");
            IList<GitLabApiClient.Models.Issues.Responses.Issue> issuesList;
            try
            {
                issuesList = await client.Issues.GetAllAsync(options.Project, options: o => o.State = GitLabApiClient.Models.Issues.Responses.IssueState.All);
            }
            catch (Exception ex)
            {
                StyledConsoleWriter.WriteError($"Error when getting issues from GitLab: {ex.Message}");
                return (false, null, null);
            }
            StyledConsoleWriter.WriteInfo($"Found {issuesList.Count} issues");

            foreach(var issue in issuesList)
            {
                issues.Add(new Models.Issue()
                {
                    Closed = issue.ClosedAt.HasValue,
                    CreatedAt = issue.CreatedAt,
                    Iid = issue.Iid,
                    LabelList = issue.Labels,
                    Milestone = issue.Milestone?.Title ?? string.Empty,
                    Spent = (double)issue.TimeStats.TotalTimeSpent / 60 / 60,
                    State = issue.State.ToString(), // todo: is this correct?
                    Title = issue.Title,
                    TotalEstimate = (double)issue.TimeStats.TimeEstimate / 60 / 60,
                    UpdatedAt = issue.UpdatedAt
                });
            }

            foreach (var issue in issues)
            {
                StyledConsoleWriter.WriteInfo($"Getting notes for {issue.Title}");
                var notesList = await client.Issues.GetNotesAsync(options.Project, issue.Iid);
                // process notes generated by system
                // todo: do we need to do this parallel? Might be an overkill...
                Parallel.ForEach(notesList.Where(n => n.System == true), (note) =>
                {
                    if (note.Body.Contains("time spent"))
                    {
                        var timeData = timeParser.GetSpentHours(note);
                        records.Add(new Models.Record()
                        {
                            Date = timeData.date,
                            Iid = issue.Iid,
                            Time = timeData.hours,
                            Type = note.NoteableType,
                            User = note.Author.Username // todo: change this when removing EFCore and reworking Models
                        });
                    }
                });
            }

            return (true, issues, records);
        }
    }
}
