using gttcharts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts
{
    public class GttDataQueryProvider
    {
        private readonly IEnumerable<Issue> issues;
        private readonly IEnumerable<Record> records;
        private readonly GttChartsOptions options;
        private readonly GttChartBuilderUtils utils;

        public GttDataQueryProvider(IEnumerable<Issue> issues, IEnumerable<Record> records, GttChartsOptions options, GttChartBuilderUtils utils)
        {
            this.issues = issues;
            this.records = records;
            this.options = options;
            this.utils = utils;
        }

        public IEnumerable<TimesPerMilestone> GetTimesPerMilestones()
        {
            return from i in issues
                   where !options.IgnoreMilestones.Contains(i.Milestone)
                   group i by i.Milestone
                   into lst
                   select new TimesPerMilestone
                   {
                       Milestone = lst.Key,
                       Estimate = utils.Round(lst.Sum(i => i.TotalEstimate)),
                       Spent = utils.Round(lst.Sum(i => i.Spent))
                   };
        }

        public IEnumerable<TimePerUser> GetTimePerUsers()
        {
            return from r in records
                   where !options.IgnoreUsers.Contains(r.User)
                   group r by r.User
                   into lst
                   select new TimePerUser
                   {
                       User = lst.Key,
                       Spent = utils.Round(lst.Sum(r => r.Time))
                   };
        }

        public IEnumerable<TimePerUserAndWeek> GetTimePerUserAndWeeks()
        {
            return from re in (from r in records
                               where !options.IgnoreUsers.Contains(r.User)
                               select new
                               {
                                   User = r.User,
                                   Time = r.Time,
                                   Week = utils.WeekNrFromDate(r.Date)
                               })
                   group re by new { re.User, re.Week }
                   into lst
                   select new TimePerUserAndWeek
                   {
                       User = lst.Key.User,
                       Week = lst.Key.Week,
                       TotalTime = lst.Sum(e => e.Time)
                   };
        }

        public IEnumerable<TimePerUserWeeks> GetTimePerUserWeeks()
        {
            return from pu in GetTimePerUserAndWeeks()
                   group pu by pu.User
                   into list
                   select new TimePerUserWeeks
                   {
                       User = list.Key,
                       Weeks = list
                   };
        }

        public IEnumerable<TimesPerLabel> GetTimesPerLabels()
        {
            utils.WarnIfIssuesWithMultipleActiveLabels();
            var labels = utils.GetLabels();
            var inflatedIssues = from i in issues
                                 from l in labels
                                 where i.LabelList.Contains(l)
                                 select new
                                 {
                                     Label = l,
                                     Issue = i
                                 };

            return from ii in inflatedIssues
                   group ii by ii.Label
                   into list
                   select new TimesPerLabel
                   {
                       Label = list.Key,
                       Estimate = utils.Round(list.Sum(i => i.Issue.TotalEstimate)),
                       Spent = utils.Round(list.Sum(i => i.Issue.Spent))
                   };
        }

        public IEnumerable<TimePerUserAndMilestone> GetTimePerUserAndMilestones()
        {
            return from r in records
                   join i in issues
                   on r.Iid equals i.Iid
                   where !options.IgnoreMilestones.Contains(i.Milestone)
                   where !options.IgnoreUsers.Contains(r.User)
                   group r by new { i.Milestone, r.User }
                   into lst
                   select new TimePerUserAndMilestone
                   {
                       User = lst.Key.User,
                       Milestone = lst.Key.Milestone,
                       TotalTime = lst.Sum(r => r.Time)
                   };
        }
    }

    public record TimesPerMilestone
    {
        public string Milestone { get; init; }
        public double Estimate { get; init; }
        public double Spent { get; init; }
    }

    public record TimePerUser
    {
        public string User { get; init; }
        public double Spent { get; init; }
    }

    public record TimePerUserAndWeek
    {
        public string User { get; init; }
        public int Week { get; init; }
        public double TotalTime { get; init; }
    }

    public record TimePerUserWeeks
    {
        public string User { get; init; }
        public IEnumerable<TimePerUserAndWeek> Weeks { get; init; }
    }

    public record TimesPerLabel
    {
        public string Label { get; init; }
        public double Estimate { get; init; }
        public double Spent { get; init; }
    }

    public record TimePerUserAndMilestone
    {
        public string User { get; init; }
        public string Milestone { get; init; }
        public double TotalTime { get; init; }
    }
}
