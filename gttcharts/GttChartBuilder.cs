using gttcharts.Models;
using Microsoft.Data.Sqlite;
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
    public class GttChartBuilder
    {
        private string InternalImageOutputFolderPath = string.Empty;
        private string RelativeMarkdownAssetFolderPath = string.Empty;

        private readonly Dictionary<string, string> chartFilePaths = new();
        private readonly Dictionary<string, string> chartFileNames = new();

        private readonly GttChartsOptions options;
        private readonly IEnumerable<Issue> issues;
        private readonly IEnumerable<Record> records;

        public readonly bool InitSuccessful;

        private readonly GttChartBuilderUtils utils;

        public GttChartBuilder(GttChartsOptions options)
        {
            this.options = options;

            if (!File.Exists(this.options.DatabasePath))
            {
                StyledConsoleWriter.WriteError($"The database file wasn't found: {this.options.DatabasePath}. Make sure you have specified the correct path in appsettings.json");
                return;
            }

            var opt = new DbContextOptionsBuilder<GttContext>().UseSqlite($"DataSource={this.options.DatabasePath};");

            try
            {
                using (var context = new GttContext(opt.Options))
                {
                    issues = context.Issues.ToList();
                    records = context.Records.ToList();
                }
                utils = new GttChartBuilderUtils(issues, records, this.options);
            }
            catch (SqliteException ex)
            {
                StyledConsoleWriter.WriteError($"An error occured when trying to load data from the database: {ex.Message}");
                StyledConsoleWriter.WriteWarning($"Make sure you have specified the correct path in appsettings.json and the database has a valid schema");
                return;
            }
            InitSuccessful = true;
        }

        #region Graphing

        public void RunAll()
        {
            if (options.HasOutputPath())
            {
                Directory.CreateDirectory(options.GetOutputPath());
                InternalImageOutputFolderPath = options.OutputDirectory;
            }

            if (options.MarkdownAssetFolder && options.CreateMarkdownOutput)
            {
                InternalImageOutputFolderPath = $"{options.GetOutputPath()}{options.MarkdownOutputName}.assets";
                RelativeMarkdownAssetFolderPath = $"{options.MarkdownOutputName}.assets";
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

            if (options.CreateMarkdownOutput)
            {
                CreateMarkdown();
            }
        }

        private void CreateMarkdown()
        {
            StringBuilder markdownBuilder = new();
            foreach (var pair in options.GttChartJobOptions.Where(kvp => kvp.Value.Create == true))
            {
                markdownBuilder.AppendLine($"## {pair.Value.Title}");
                markdownBuilder.AppendLine($"![]({RelativeMarkdownAssetFolderPath}/{chartFileNames[pair.Key]})");
            }

            File.WriteAllText($"{options.OutputDirectory}/{options.MarkdownOutputName}.md", markdownBuilder.ToString());
            StyledConsoleWriter.WriteSuccess($"Created Markdown Output --> {options.OutputDirectory}/{options.MarkdownOutputName}.md");
        }


        private void CreateTimePerMilestone(Plot plt)
        {
            var perMilestone = from i in issues
                               where !options.IgnoreMilestones.Contains(i.Milestone)
                               group i by i.Milestone
                               into lst
                               select new
                               {
                                   Milestone = lst.Key,
                                   Estimate = utils.Round(lst.Sum(i => i.TotalEstimate)),
                                   Spent = utils.Round(lst.Sum(i => i.Spent))
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
            var perIssue = this.issues.Where(i => i.TotalEstimate > 0 || !options.IgnoreEmptyIssues);
            string[] issues = perIssue.Select(i => i.Title).ToArray();
            double[] estimates = perIssue.Select(p => utils.Round(p.TotalEstimate)).ToArray();
            double[] spent = perIssue.Select(p => utils.Round(p.Spent)).ToArray();

            plt.PlotBarGroups(
                issues,
                new string[] { "estimated", "spent" },
                new double[][] { estimates, spent },
                showValues: true);

            plt.Ticks(xTickRotation: 45);
        }

        private void CreateTimePerUser(Plot plt)
        {
            var perUser = from r in records
                          where !options.IgnoreUsers.Contains(r.User)
                          group r by r.User
                          into lst
                          select new
                          {
                              User = lst.Key,
                              Spent = utils.Round(lst.Sum(r => r.Time))
                          };

            string[] users = utils.GetMappedNames(perUser.Select(p => p.User).ToArray());
            double[] spent = perUser.Select(p => p.Spent).ToArray();

            plt.PlotPie(spent, users, showLabels: false, showValues: true);

            plt.Grid(false);
            plt.Frame(false);
            plt.Ticks(false, false);
        }

        private void CreateTimePerUserPerWeekArea(Plot plt)
        {
            var perUserAndWeek = from re in (from r in records
                                             where !options.IgnoreUsers.Contains(r.User)
                                             select new
                                             {
                                                 User = r.User,
                                                 Time = r.Time,
                                                 Week = utils.WeekNrFromDate(r.Date)
                                             })
                                 group re by new { re.User, re.Week }
                                 into lst
                                 select new
                                 {
                                     User = lst.Key.User,
                                     Week = lst.Key.Week,
                                     TotalTime = lst.Sum(e => e.Time)
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
            double[] cumulatedYValues = new double[utils.GetTotalWeekCount()];

            // contains all Y values
            List<double[]> yValuesList = new();

            foreach (var pu in perUser)
            {
                double[] perUserYValues = new double[utils.GetTotalWeekCount()];
                for (int i = 0; i < utils.GetTotalWeekCount(); i++)
                {
                    // we don't need to round here since the values aren't displayed in the chart
                    perUserYValues[i] = pu.Weeks.Where(w => w.Week == i + 1).Sum(w => w.TotalTime);
                }
                for (int i = 0; i < perUserYValues.Length; i++)
                {
                    // offset from 'lower' lines is added
                    perUserYValues[i] += cumulatedYValues[i];
                    cumulatedYValues[i] = perUserYValues[i];
                }
                yValuesList.Add(perUserYValues);
            }

            // yValuesList is reversed in order to 'layer' the filled areas accordingly so colors are displayed correctly
            yValuesList.Reverse();
            int puIndex = 0;
            foreach (var ys in yValuesList)
            {
                var sig = plt.PlotSignal(ys, label: utils.GetMappedName(perUser.ElementAt(puIndex++).User));
                sig.fillType = FillType.FillBelow;
                sig.fillColor1 = sig.color;
                sig.gradientFillColor1 = sig.color;
            }

            plt.XLabel("Week");
            plt.YLabel("Hours");
            plt.XTicks(Enumerable.Range(1, utils.GetTotalWeekCount()).Select((i) => $"Week {i}").ToArray());
            plt.AxisBounds(minY: 0);
        }

        private void CreateTimePerUserPerWeekBar(Plot plt)
        {
            // todo: remove duplicate code
            var perUserAndWeek = from re in (from r in records
                                             where !options.IgnoreUsers.Contains(r.User)
                                             select new
                                             {
                                                 User = r.User,
                                                 Time = r.Time,
                                                 Week = utils.WeekNrFromDate(r.Date)
                                             })
                                 group re by new { re.User, re.Week }
                     into lst
                                 select new
                                 {
                                     User = lst.Key.User,
                                     Week = lst.Key.Week,
                                     TotalTime = lst.Sum(e => e.Time)
                                 };

            var perUser = from pu in perUserAndWeek
                          group pu by pu.User
                          into list
                          select new
                          {
                              User = list.Key,
                              Weeks = list
                          };

            var users = utils.GetUsernames();
            var datapoints = new double[users.Length][];
            // todo: consider refactoring this into foreach->for loop, would reduce complexity of LINQ statement
            for (int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[utils.GetTotalWeekCount()];
                for (int j = 0; j < utils.GetTotalWeekCount(); j++)
                {
                    datapoints[i][j] = utils.Round(perUser.Where(pu => pu.User == users[i]).Sum(wks => wks.Weeks.Sum(w => w.TotalTime)));
                }
            }

            plt.PlotBarGroups(
                Enumerable.Range(1, utils.GetTotalWeekCount()).Select((i) => $"Week {i}").ToArray(),
                utils.GetMappedNames(users),
                datapoints
                );
        }

        private void CreateTimePerLabelBar(Plot plt)
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

            var perLabel = from ii in inflatedIssues
                           group ii by ii.Label
                           into list
                           select new
                           {
                               Label = list.Key,
                               Estimate = utils.Round(list.Sum(i => i.Issue.TotalEstimate)),
                               Spent = utils.Round(list.Sum(i => i.Issue.Spent))
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
            // todo: duplicate code 
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

            var perLabel = from ii in inflatedIssues
                           group ii by ii.Label
                           into list
                           select new
                           {
                               Label = list.Key,
                               Estimate = utils.Round(list.Sum(i => i.Issue.TotalEstimate)),
                               Spent = utils.Round(list.Sum(i => i.Issue.Spent))
                           };

            double[] spents = perLabel.Select(p => p.Spent).ToArray();

            plt.PlotPie(spents, labels, showLabels: false, showValues: true);

            plt.Grid(false);
            plt.Frame(false);
            plt.Ticks(false, false);
        }

        private void CreateUserPerMilestone(Plot plt)
        {
            var perUserAndMilestone = from r in records
                                      join i in issues
                                      on r.Iid equals i.Iid
                                      where !options.IgnoreMilestones.Contains(i.Milestone)
                                      where !options.IgnoreUsers.Contains(r.User)
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

            var users = utils.GetUsernames();
            var datapoints = new double[users.Length][];
            for (int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[utils.GetMilestones().Length];
                for (int j = 0; j < datapoints[i].Length; j++)
                {
                    datapoints[i][j] = utils.Round(
                        perUser.Where(pu => pu.User == users[i])
                        .Sum(ms => 
                            ms.Milestones.Where(m => m.Milestone == utils.GetMilestones()[j])
                            .Sum(rs => rs.Records.Sum(r => r.Time))
                            )
                        );
                }
            }

            plt.PlotBarGroups(
                utils.GetMilestones(),
                utils.GetMappedNames(users),
                datapoints,
                showValues: true);
        }

        private void CreateMilestonePerUser(Plot plt)
        {
            // todo: duplicate code
            var perUserAndMilestone = from r in records
                                      join i in issues
                                      on r.Iid equals i.Iid
                                      where !options.IgnoreMilestones.Contains(i.Milestone)
                                      where !options.IgnoreUsers.Contains(r.User)
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
            var milestones = utils.GetMilestones();
            var datapoints = new double[milestones.Length][];

            for (int i = 0; i < milestones.Length; i++)
            {
                datapoints[i] = new double[utils.GetUsernames().Length];
                for (int j = 0; j < datapoints[i].Length; j++)
                {
                    datapoints[i][j] = utils.Round(
                        perMilestone.Where(pm => pm.Milestone == milestones[i])
                        .Sum(us => 
                            us.Users.Where(u => u.User == utils.GetUsernames()[j])
                            .Sum(rs => rs.Records.Sum(r => r.Time))
                            )
                        );
                }
            }
            plt.PlotBarGroups(
                utils.GetMappedNames(),
                milestones,
                datapoints,
                showValues: true);
        }

        #endregion Graphing

        private void CreateGraph(Action<Plot> plot, string name)
        {
            var jobOptions = options.GttChartJobOptions[name];
            if (!jobOptions.Create)
            {
                StyledConsoleWriter.WriteInfo($"Skipping chart job {name} as per settings");
                return;
            }

            var plt = new ScottPlot.Plot(jobOptions.PlotWidth, jobOptions.PlotHeight);

            // call custom plot function for each job
            plot(plt);

            // apply jobOptions which are defined in appsettings.json
            plt.Title(jobOptions.Title);
            plt.XLabel(jobOptions.XLabel);
            plt.YLabel(jobOptions.YLabel);
            plt.Layout(yScaleWidth: jobOptions.YScaleWidth, xScaleHeight: jobOptions.XScaleHeight);

            plt.Legend(location: legendLocation.upperRight);


            string path = $"./{InternalImageOutputFolderPath}/{jobOptions.Filename}.png";
            plt.SaveFig(path);
            chartFilePaths.Add(name, path);
            chartFileNames.Add(name, $"{name}.png");
            StyledConsoleWriter.WriteSuccess($"Created {name}.png -> {path}");
        }
    }
}
