﻿using gttcharts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gttcharts
{
    public class GttChartsBuilder
    {
        private string InternalImageOutputFolderPath = string.Empty;
        private string RelativeMarkdownAssetFolderPath = string.Empty;

        // todo: this could potentially be loaded from appsettings
        private Dictionary<string, string> ChartTitles = new()
        {
            { "PerIssue", "Time per Issue [hours]" },
            { "PerMilestone", "Time per Milestone [hours], estimate vs. recorded time" },
            { "PerUser", "Time per User [hours]" },
            { "PerUserPerWeekArea", "Time per User per Week (Area)" },
            { "PerUserPerWeekBar", "Time per User per Week (Bar)" },
            { "PerLabelBar", "Time per Label (Bar), estimate vs. recorded time" },
            { "PerLabelPie", "Time per Label (Pie) [hours]" },
            { "UserPerMilestone", "Time per User per Milestone (Bar) [hours]" },
            { "MilestonePerUser", "Time per Milestone per User (Bar) [hours]" },
        };

        private Dictionary<string, string> ChartFilePaths = new();
        private Dictionary<string, string> ChartFileNames = new();

        private readonly GttChartsOptions _options;
        private readonly ICollection<Issue> _issues;
        private readonly ICollection<Record> _records;

        public GttChartsBuilder(GttChartsOptions options)
        {
            _options = options;
            var opt = new DbContextOptionsBuilder<GttContext>().UseSqlite($"DataSource={_options.DatabasePath};");

            using (var context = new GttContext(opt.Options))
            {
                _issues = context.Issues.ToList();
                _records = context.Records.ToList();
            }
        }

        #region Graphing

        public void RunAll()
        {
            if (_options.OutputDirectory is not "" && _options.OutputDirectory is not null)
            {
                Directory.CreateDirectory(_options.OutputDirectory);
                InternalImageOutputFolderPath = _options.OutputDirectory;
            }

            if (_options.MarkdownAssetFolder && _options.CreateMarkdownOutput)
            {
                InternalImageOutputFolderPath = $"{((_options.OutputDirectory is not "" && _options.OutputDirectory is not null) ? $"{_options.OutputDirectory}/" : string.Empty)}{_options.MarkdownOutputName}.assets";
                RelativeMarkdownAssetFolderPath = $"{_options.MarkdownOutputName}.assets";
                Directory.CreateDirectory(InternalImageOutputFolderPath);
            }

            // todo: extract to dict to map names to functions
            CreateGraph(CreateTimePerIssue, "PerIssue");
            CreateGraph(CreateTimePerMilestone, "PerMilestone");
            CreateGraph(CreateTimePerUser, "PerUser");
            CreateGraph(CreateTimePerUserPerWeekArea, "PerUserPerWeekArea");
            CreateGraph(CreateTimePerUserPerWeekBar, "PerUserPerWeekBar");
            CreateGraph(CreateTimePerLabelBar, "PerLabelBar");
            CreateGraph(CreateTimePerLabelPie, "PerLabelPie");
            CreateGraph(CreateUserPerMilestone, "UserPerMilestone");
            CreateGraph(CreateMilestonePerUser, "MilestonePerUser");

            if (_options.CreateMarkdownOutput)
            {
                CreateMarkdown();
            }
        }

        private void CreateMarkdown()
        {
            StringBuilder markdownBuilder = new();
            foreach (var pair in ChartTitles)
            {
                markdownBuilder.AppendLine($"## {pair.Value}");
                markdownBuilder.AppendLine($"![]({RelativeMarkdownAssetFolderPath}/{ChartFileNames[pair.Key]})");
            }

            File.WriteAllText($"{_options.OutputDirectory}/{_options.MarkdownOutputName}.md", markdownBuilder.ToString());
        }


        private void CreateTimePerMilestone(Plot plt)
        {
            var perMilestone = from i in _issues
                               where !_options.IgnoreMilestones.Contains(i.Milestone)
                               group i by i.Milestone
                               into lst
                               select new
                               {
                                   Milestone = lst.Key,
                                   Estimate = Round(lst.Sum(i => i.TotalEstimate)),
                                   Spent = Round(lst.Sum(i => i.Spent))
                               };

            string[] milestonenames = perMilestone.Select(i => i.Milestone).ToArray();
            double[] estimates = perMilestone.Select(p => p.Estimate).ToArray();
            double[] spent = perMilestone.Select(p => p.Spent).ToArray();

            plt.PlotBarGroups(
                milestonenames,
                new string[] { "estimated", "spent" },
                new double[][] { estimates, spent },
                showValues: true);
        }

        private void CreateTimePerIssue(Plot plt)
        {
            var perIssue = _issues.Where(i => i.TotalEstimate > 0 || !_options.IgnoreEmptyIssues);
            string[] issues = perIssue.Select(i => i.Title).ToArray();
            double[] estimates = perIssue.Select(p => Round(p.TotalEstimate)).ToArray();
            double[] spent = perIssue.Select(p => Round(p.Spent)).ToArray();

            plt.PlotBarGroups(
                issues,
                new string[] { "estimated", "spent" },
                new double[][] { estimates, spent },
                showValues: true);

            plt.Ticks(xTickRotation: 90);
        }

        private void CreateTimePerUser(Plot plt)
        {
            var perUser = from r in _records
                          group r by r.User
                          into lst
                          select new
                          {
                              User = lst.Key,
                              Spent = Round(lst.Sum(r => r.Time))
                          };

            string[] users = perUser.Select(p => p.User).ToArray();
            double[] spent = perUser.Select(p => p.Spent).ToArray();

            plt.PlotPie(spent, users, showLabels: false, showValues: true);

            plt.Grid(false);
            plt.Frame(false);
            plt.Ticks(false, false);
        }

        private void CreateTimePerUserPerWeekArea(Plot plt)
        {
            var perUserAndWeek = from re in (from r in _records
                                             select new
                                             {
                                                 Record = r,
                                                 Week = WeekNrFromDate(r.Date)
                                             })
                                 group re by new { re.Record.User, re.Week }
                                 into lst
                                 select new
                                 {
                                     User = lst.Key.User,
                                     Week = lst.Key.Week,
                                     Records = lst
                                 };

            var perUser = from pu in perUserAndWeek
                          group pu by pu.User
                          into list
                          select new
                          {
                              User = list.Key,
                              Weeks = list
                          };

            // is used to offset later calculated Y-Value
            double[] baseYs = new double[GetTotalWeekCount()];
            List<double[]> yss = new();

            foreach (var pu in perUser)
            {
                // todo: fix totalWeekCount
                double[] ys = new double[GetTotalWeekCount()];
                for (int i = 0; i < GetTotalWeekCount(); i++)
                {
                    ys[i] = pu.Weeks.Where(w => w.Week == i + 1).Sum(w => w.Records.Sum(r => r.Record.Time));
                }
                for (int i = 0; i < ys.Length; i++)
                {
                    // offset from 'lower' lines is added
                    ys[i] += baseYs[i];
                    baseYs[i] = ys[i];
                }
                yss.Add(ys);
            }

            // yss is reversed in order to 'layer' the filled areas accordingly so colors are displayed correctly
            yss.Reverse();
            int puIndex = 0;
            foreach (var ys in yss)
            {
                var sig = plt.PlotSignal(ys, label: perUser.ElementAt(puIndex++).User);
                sig.fillType = FillType.FillBelow;
                sig.fillColor1 = sig.color;
                sig.gradientFillColor1 = sig.color;
            }

            plt.XLabel("Week");
            plt.YLabel("Hours");
            plt.XTicks(Enumerable.Range(1, GetTotalWeekCount()).Select((i) => $"Week {i}").ToArray());
            plt.AxisBounds(minY: 0);
        }

        private void CreateTimePerUserPerWeekBar(Plot plt)
        {
            // todo: remove duplicate code
            var perUserAndWeek = from re in (from r in _records
                                             select new
                                             {
                                                 Record = r,
                                                 Week = WeekNrFromDate(r.Date)
                                             })
                                 group re by new { re.Record.User, re.Week }
                                 into lst
                                 select new
                                 {
                                     User = lst.Key.User,
                                     Week = lst.Key.Week,
                                     Records = lst
                                 };

            var perUser = from pu in perUserAndWeek
                          group pu by pu.User
                          into list
                          select new
                          {
                              User = list.Key,
                              Weeks = list
                          };

            var users = GetUsernames();
            var datapoints = new double[users.Length][];
            for (int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[GetTotalWeekCount()];
                for (int j = 0; j < GetTotalWeekCount(); j++)
                {
                    datapoints[i][j] = Round(perUser.Where(pu => pu.User == users[i]).Sum(wks => wks.Weeks.Where(w => w.Week == j + 1).Sum(rs => rs.Records.Sum(r => r.Record.Time))));
                }
            }

            plt.PlotBarGroups(
                Enumerable.Range(1, GetTotalWeekCount()).Select((i) => $"Week {i}").ToArray(),
                users,
                datapoints
                );
        }

        private void CreateTimePerLabelBar(Plot plt)
        {
            // count issues that have more than one label which isn't ignored
            if (_issues.Count(i => i.LabelList.Except(_options.IgnoreLabels).Count() > 0) > 0)
            {
                Console.WriteLine($"Warning: there are issues that have more than one label to be reported on. This might lead to skewed display of times.");
            }
            var labels = GetLabels();
            var inflatedIssues = from i in _issues
                                 from l in labels
                                 where i.LabelList.Contains(l)
                                 select new
                                 {
                                     Label = l,
                                     Issue = i
                                 };

            var perLabel = from ii in inflatedIssues
                           group ii by ii.Label
                           into list
                           select new
                           {
                               Label = list.Key,
                               Estimate = Round(list.Sum(i => i.Issue.TotalEstimate)),
                               Spent = Round(list.Sum(i => i.Issue.Spent))
                           };

            double[] estimates = perLabel.Select(p => p.Estimate).ToArray();
            double[] spents = perLabel.Select(p => p.Spent).ToArray();

            plt.PlotBarGroups(
                labels,
                new string[] { "estimated", "spent" },
                new double[][] { estimates, spents },
                showValues: true);

        }

        private void CreateTimePerLabelPie(Plot plt)
        {
            // todo: remove duplicate code
            // count issues that have more than one label which isn't ignored
            if (_issues.Count(i => i.LabelList.Except(_options.IgnoreLabels).Count() > 0) > 0)
            {
                Console.WriteLine($"Warning: there are issues that have more than one label to be reported on. This might lead to skewed display of times.");
            }
            var labels = GetLabels();
            var inflatedIssues = from i in _issues
                                 from l in labels
                                 where i.LabelList.Contains(l)
                                 select new
                                 {
                                     Label = l,
                                     Issue = i
                                 };

            var perLabel = from ii in inflatedIssues
                           group ii by ii.Label
                           into list
                           select new
                           {
                               Label = list.Key,
                               Estimate = Round(list.Sum(i => i.Issue.TotalEstimate)),
                               Spent = Round(list.Sum(i => i.Issue.Spent))
                           };

            double[] spents = perLabel.Select(p => p.Spent).ToArray();

            plt.PlotPie(spents, labels, showLabels: false, showValues: true);

            plt.Grid(false);
            plt.Frame(false);
            plt.Ticks(false, false);
        }

        private void CreateUserPerMilestone(Plot plt)
        {
            var perUserAndMilestone = from r in _records
                                      join i in _issues
                                      on r.Iid equals i.Iid
                                      where !_options.IgnoreMilestones.Contains(i.Milestone)
                                      group r by new { i.Milestone, r.User }
                                      into lst
                                      select new
                                      {
                                          User = lst.Key.User,
                                          Milestone = lst.Key.Milestone,
                                          Records = lst
                                      };

            var perUser = from pu in perUserAndMilestone
                          group pu by pu.User
                          into list
                          select new
                          {
                              User = list.Key,
                              Milestones = list
                          };

            var users = GetUsernames();
            var datapoints = new double[users.Length][];
            for (int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[GetMilestones().Length];
                for (int j = 0; j < datapoints[i].Length; j++)
                {
                    datapoints[i][j] = Round(perUser.Where(pu => pu.User == users[i]).Sum(ms => ms.Milestones.Where(m => m.Milestone == GetMilestones()[j]).Sum(rs => rs.Records.Sum(r => r.Time))));
                }
            }

            plt.PlotBarGroups(
                GetMilestones(),
                users,
                datapoints,
                showValues: true);
        }

        private void CreateMilestonePerUser(Plot plt)
        {
            var perUserAndMilestone = from r in _records
                                      join i in _issues
                                      on r.Iid equals i.Iid
                                      where !_options.IgnoreMilestones.Contains(i.Milestone)
                                      group r by new { i.Milestone, r.User }
                                      into lst
                                      select new
                                      {
                                          User = lst.Key.User,
                                          Milestone = lst.Key.Milestone,
                                          Records = lst
                                      };

            var perMilestone = from pu in perUserAndMilestone
                               group pu by pu.Milestone
                               into list
                               select new
                               {
                                   Milestone = list.Key,
                                   Users = list
                               };
            var milestones = GetMilestones();
            var datapoints = new double[milestones.Length][];

            for (int i = 0; i < milestones.Length; i++)
            {
                datapoints[i] = new double[GetUsernames().Length];
                for (int j = 0; j < datapoints[i].Length; j++)
                {
                    datapoints[i][j] = Round(perMilestone.Where(pm => pm.Milestone == milestones[i]).Sum(us => us.Users.Where(u => u.User == GetUsernames()[j]).Sum(rs => rs.Records.Sum(r => r.Time))));
                }
            }
            plt.PlotBarGroups(
                GetUsernames(),
                milestones,
                datapoints,
                showValues: true);
        }

        #endregion Graphing

        #region HelperFunctions

        private string[] GetUsernames()
        {
            return (from r in _records
                    where !_options.IgnoreUsers.Contains(r.User)
                    group r by r.User
                        into lst
                    select new
                    {
                        User = lst.Key
                    }).Select(u => u.User).ToArray();
        }

        private string[] GetMilestones()
        {
            return (from i in _issues
                    where !_options.IgnoreMilestones.Contains(i.Milestone)
                    group i by i.Milestone
                    into lst
                    select new
                    {
                        Milestone = lst.Key
                    }).Select(m => m.Milestone).ToArray();
        }

        private string[] GetLabels()
        {
            return _issues.SelectMany(i => i.LabelList).Distinct().Except(_options.IgnoreLabels).ToArray();
        }

        private void CreateGraph(Action<Plot> plot, string name)
        {
            var plt = new ScottPlot.Plot(_options.PlotWidth, _options.PlotHeight);

            plot(plt);
            plt.Title(ChartTitles[name]);
            plt.Legend(location: legendLocation.upperRight);

            string path = $"./{InternalImageOutputFolderPath}/{name}.png";
            plt.SaveFig(path);
            ChartFilePaths.Add(name, path);
            ChartFileNames.Add(name, $"{name}.png");
            Console.WriteLine($"Created {name}.png -> {path}");
        }

        private double Round(double d)
        {
            return Math.Round(d, _options.RoundToDecimals);
        }

        private int WeekNrFromDate(DateTime date)
        {
            // always compare to end of project week
            DateTime runner = _options.ProjectStart.AddDays(7);
            int weeknr = 1;
            while (runner < date)
            {
                runner = runner.AddDays(7);
                weeknr++;
            }

            return weeknr;
        }

        private int GetTotalWeekCount()
        {
            return WeekNrFromDate(_options.ProjectEnd);
        }

        #endregion
    }
}