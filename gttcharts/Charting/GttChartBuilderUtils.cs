using gttcharts.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gttcharts.Charting
{
    public class GttChartBuilderUtils
    {
        private readonly IEnumerable<Issue> issues;
        private readonly IEnumerable<Record> records;
        private readonly GttChartsOptions options;

        public GttChartBuilderUtils(IEnumerable<Issue> issues, IEnumerable<Record> records, GttChartsOptions options)
        {
            this.issues = issues;
            this.records = records;
            this.options = options;
        }

        public string[] GetUsernames()
        {
            return (from r in records
                    where !options.IgnoreUsers.Contains(r.User)
                    group r by r.User
                        into lst
                    orderby lst.Key ascending
                    select new
                    {
                        User = lst.Key
                    }).Select(u => u.User).ToArray();
        }

        public string[] GetMappedNames()
        {
            return GetMappedNames(GetUsernames());
        }

        public string[] GetMappedNames(string[] usernames)
        {
            if (options.UsernameMapping.Count == 0)
            {
                return usernames;
            }

            string[] names = new string[usernames.Length];

            for (int i = 0; i < usernames.Length; i++)
            {
                names[i] = GetMappedName(usernames[i]);
            }

            return names;
        }

        public string GetMappedName(string username)
        {
            if (!options.UsernameMapping.TryGetValue(username, out string name))
            {
                StyledConsoleWriter.WriteWarning($"There was no mapping for username [{username}] provided. Consider doing so in gttchartsettings.json");
                name = username;
            }
            return name;
        }

        public string[] GetMilestones()
        {
            // if settings contain the milestones (in order), use those
            if (options.MilestonesInOrder.Count > 0)
            {
                return options.MilestonesInOrder.Except(options.IgnoreMilestones).ToArray();
            }

            return (from i in issues
                    where !options.IgnoreMilestones.Contains(i.Milestone)
                    group i by i.Milestone
                    into lst
                    select new
                    {
                        Milestone = lst.Key
                    }).Select(m => m.Milestone).ToArray();
        }

        public string[] GetLabels()
        {
            return issues.SelectMany(i => i.LabelList).Distinct().Except(options.IgnoreLabels).ToArray();
        }

        public double Round(double d)
        {
            return Math.Round(d, options.RoundToDecimals);
        }

        public int WeekNrFromDate(DateTime date)
        {
            // always compare to end of project week
            DateTime runner = options.ProjectStart.AddDays(7);
            int weeknr = 1;
            while (runner <= date)
            {
                runner = runner.AddDays(7);
                weeknr++;
            }

            return weeknr;
        }

        public int GetTotalWeekCount()
        {
            return WeekNrFromDate(options.ProjectEnd);
        }

        /// <summary>
        /// Checks to see if there are issues in the dataset that have more than one label
        /// which isn't ignored. This might be important, because it potentially skews
        /// charts in which total time for labels is visualized
        /// </summary>
        public void WarnIfIssuesWithMultipleActiveLabels()
        {
            if (issues.Count(i => i.LabelList.Except(options.IgnoreLabels).Count() > 1) > 0)
            {
                StyledConsoleWriter.WriteWarning($"Warning: there are issues that have more than one label to be reported on. This might lead to skewed display of times.");

                // tell use which issues are affected by this, so they can see the labels
                // and consider adding some of these labels to the ignore list
                StyledConsoleWriter.WriteInfo($"The following issues will be reported multiple times:");
                foreach (var issue in issues.Where(i => i.LabelList.Except(options.IgnoreLabels).Count() > 1))
                {
                    StyledConsoleWriter.WriteInfo($"Issue #{issue.Iid}: {issue.Title} | Labels: {issue.LabelList.Except(options.IgnoreLabels).Aggregate((a, b) => $"{a}, {b}")}");
                }
            }
        }
    }
}
