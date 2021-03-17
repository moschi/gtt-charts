using gttcharts.Data;
using gttcharts.Models;
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

namespace gttcharts.Charting
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

        private readonly GttChartBuilderUtils utils;
        private readonly GttDataQueryProvider dataProvider;

        public GttChartBuilder(GttChartsOptions options, IEnumerable<Issue> issues, IEnumerable<Record> records)
        {
            this.options = options;
                this.issues = issues;
                this.records = records;
                utils = new GttChartBuilderUtils(issues, records, this.options);
                dataProvider = new GttDataQueryProvider(issues, records, options, utils);
        }

        #region Graphing

        public void RunAll()
        {
            StyledConsoleWriter.WriteInfo("Creating charts...");
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
            var perMilestone = dataProvider.GetTimesPerMilestones();

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
            //double[] actualSpent = perIssue.Select(p => utils.Round(records.Where(r => r.Iid == p.Iid).Sum(r => r.Time))).ToArray();

            /*
             new string[] { "estimated", "spent", "actualspent" },
                new double[][] { estimates, spent, actualSpent },
             */
            plt.PlotBarGroups(
                issues,
                new string[] { "estimated", "spent" },
                new double[][] { estimates, spent },
                showValues: true);

            plt.Ticks(xTickRotation: 45);
        }

        private void CreateTimePerUser(Plot plt)
        {
            var perUser = dataProvider.GetTimePerUsers();

            string[] users = utils.GetMappedNames(perUser.Select(p => p.User).ToArray());
            double[] spent = perUser.Select(p => p.Spent).ToArray();

            plt.PlotPie(spent, users, showLabels: false, showValues: true);

            plt.Grid(false);
            plt.Frame(false);
            plt.Ticks(false, false);
        }

        private void CreateTimePerUserPerWeekArea(Plot plt)
        {
            var perUserWeeks = dataProvider.GetTimePerUserWeeks();

            // is used to offset later calculated Y-Value
            double[] cumulatedYValues = new double[utils.GetTotalWeekCount()];

            // contains all Y values
            List<double[]> yValuesList = new();

            // walk through it reversed, so that alphabetical order of usernames is kept
            foreach (var pu in perUserWeeks.Reverse())
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
            // no need to reverse-walk usernames, since we've reversed the original data list twice
            int puIndex = 0;
            foreach (var ys in yValuesList)
            {
                var sig = plt.PlotSignal(ys, label: utils.GetMappedName(perUserWeeks.ElementAt(puIndex++).User));
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
            var perUserAndWeek = dataProvider.GetTimePerUserAndWeeks();

            var users = utils.GetUsernames();
            var datapoints = new double[users.Length][];
            for (int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[utils.GetTotalWeekCount()];
                for (int j = 0; j < utils.GetTotalWeekCount(); j++)
                {
                    datapoints[i][j] = utils.Round(perUserAndWeek.Where(p => p.User == users[i] && p.Week == j + 1).Sum(w => w.TotalTime));
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
            var labels = utils.GetLabels();
            var perLabel = dataProvider.GetTimesPerLabels();

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
            var labels = utils.GetLabels();
            var perLabel = dataProvider.GetTimesPerLabels();

            double[] spents = perLabel.Select(p => p.Spent).ToArray();

            plt.PlotPie(spents, labels, showLabels: false, showValues: true);

            plt.Grid(false);
            plt.Frame(false);
            plt.Ticks(false, false);
        }

        private void CreateUserPerMilestone(Plot plt)
        {
            var perMilestoneAndUser = dataProvider.GetTimePerUserAndMilestones();
            var users = utils.GetUsernames();
            var datapoints = new double[users.Length][];
            for (int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[utils.GetMilestones().Length];
                for (int j = 0; j < datapoints[i].Length; j++)
                {
                    datapoints[i][j] = utils.Round(
                        perMilestoneAndUser
                            .Where(p => p.User == users[i] && p.Milestone == utils.GetMilestones()[j])
                            .Sum(rs => rs.TotalTime));
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
            var perMilestoneAndUser = dataProvider.GetTimePerUserAndMilestones();
            var milestones = utils.GetMilestones();
            var datapoints = new double[milestones.Length][];

            for (int i = 0; i < milestones.Length; i++)
            {
                datapoints[i] = new double[utils.GetUsernames().Length];
                for (int j = 0; j < datapoints[i].Length; j++)
                {
                    datapoints[i][j] = utils.Round(
                        perMilestoneAndUser
                            .Where(p => p.Milestone == milestones[i] && p.User == utils.GetUsernames()[j])
                            .Sum(rs => rs.TotalTime));
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

            // apply jobOptions which are defined in gttchartsettings.json
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
