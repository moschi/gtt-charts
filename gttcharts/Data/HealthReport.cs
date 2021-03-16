using gttcharts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts.Data
{
    public class HealthReport
    {
        private readonly HealthReportOptions options;
        private readonly IEnumerable<Issue> issues;
        private readonly IEnumerable<Record> records;

        public HealthReport(HealthReportOptions options, IEnumerable<Issue> issues, IEnumerable<Record> records)
        {
            this.options = options;
            this.issues = issues;
            this.records = records;
        }

        public void PrintReport()
        {
            StyledConsoleWriter.WriteSuccess("Beginning with health report...");
            foreach (var issue in issues)
            {
                var issueRecords = records.Where(r => r.Iid == issue.Iid);
                StyledConsoleWriter.WriteInfo($"Checking data for issue #{issue.Iid}: {issue.Title}");
                CheckHappeningDate(issue, issueRecords);
                CheckMinimumDate(issue, issueRecords);
                CheckMaximumDate(issue, issueRecords);
                CheckZeroDateEntries(issue, issueRecords);
            }
            StyledConsoleWriter.WriteSuccess("Healthreport finished!");
        }

        private void CheckHappeningDate(Issue issue, IEnumerable<Record> issueRecords)
        {
            if (options.IssueHappeningDateMap.Any(e => e.Key == issue.Iid.ToString()))
            {
                DateTime issueHappeningDate = options.IssueHappeningDateMap[issue.Iid.ToString()];
                if (issueRecords.Any(r => r.Date != issueHappeningDate))
                {
                    StyledConsoleWriter.WriteError($"Found {issueRecords.Count(r => r.Date != issueHappeningDate)} that don't match the happening date!");
                    foreach (var record in issueRecords.Where(r => r.Date != issueHappeningDate))
                    {
                        StyledConsoleWriter.WriteWarning($"{record.User} on {record.Date} spent {record.Time} hours");
                    }
                }
            }
        }

        private void CheckMinimumDate(Issue issue, IEnumerable<Record> issueRecords)
        {
            if (options.IssueMinimumDate.Any(e => e.Key == issue.Iid.ToString()))
            {
                DateTime issueMinimumDate = options.IssueMinimumDate[issue.Iid.ToString()];
                if (issueRecords.Any(r => r.Date < issueMinimumDate))
                {
                    StyledConsoleWriter.WriteError($"Found {issueRecords.Count(r => r.Date < issueMinimumDate)} that are before the minimal date!");
                    foreach (var record in issueRecords.Where(r => r.Date < issueMinimumDate))
                    {
                        StyledConsoleWriter.WriteWarning($"{record.User} on {record.Date} spent {record.Time} hours");
                    }
                }
            }
        }

        private void CheckMaximumDate(Issue issue, IEnumerable<Record> issueRecords)
        {
            if (options.IssueMaximumDate.Any(e => e.Key == issue.Iid.ToString()))
            {
                DateTime issueMaximumDate = options.IssueMinimumDate[issue.Iid.ToString()];
                if (issueRecords.Any(r => r.Date > issueMaximumDate))
                {
                    StyledConsoleWriter.WriteError($"Found {issueRecords.Count(r => r.Date > issueMaximumDate)} that are after the maximum date!");
                    foreach (var record in issueRecords.Where(r => r.Date > issueMaximumDate))
                    {
                        StyledConsoleWriter.WriteWarning($"{record.User} on {record.Date} spent {record.Time} hours");
                    }
                }
            }
        }

        private void CheckZeroDateEntries(Issue issue, IEnumerable<Record> issueRecords)
        {
            if (options.CheckZeroDateEntries)
            {
                if (issueRecords.Any(r => r.Date == new DateTime()))
                {
                    StyledConsoleWriter.WriteError($"Found {issueRecords.Count(r => r.Date == new DateTime())} that have date {new DateTime()}!");
                    foreach (var record in issueRecords.Where(r => r.Date == new DateTime()))
                    {
                        StyledConsoleWriter.WriteWarning($"{record.User}; NoteBody: {record.NoteBody}");
                    }
                }
            }
        }

    }
}
