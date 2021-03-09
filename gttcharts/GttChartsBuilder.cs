using gttcharts.Models;
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
using System.Threading.Tasks;

namespace gttcharts
{
    public class GttChartsBuilder
    {
        // todo: this could potentially be loaded from appsettings
        private Dictionary<string, string> ChartTitles = new()
        {
            { "PerIssue", "Time per Issue [hours]" },
            { "PerMilestone", "Time per Milestone [hours], estimate vs. recorded time" },
            { "PerUser", "Time per User [hours]" },
            { "PerUserPerWeekArea", "Time per User per Week (Area)" },
            { "PerUserPerWeekBar", "Time per User per Week (Bar)" },
            { "PerLabelBar", "Time per Label (Bar), estimate vs. recorded time" },
            { "PerLabelPie", "Time per Label (Bar) [hours], estimate vs. recorded time" },
        };

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
            }

            // todo: extract to dict to map names to functions
            CreateGraph(CreateTimePerIssue, "PerIssue");
            CreateGraph(CreateTimePerMilestone, "PerMilestone");
            CreateGraph(CreateTimePerUser, "PerUser");
            CreateGraph(CreateTimePerUserPerWeekArea, "PerUserPerWeekArea");
            CreateGraph(CreateTimePerUserPerWeekBar, "PerUserPerWeekBar");

            // todo: PerLabelBar
            // todo: PerLabelPie
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

            plt.Legend(location: legendLocation.upperRight);
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
            plt.Legend(location: legendLocation.upperRight);
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
            plt.Legend();

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

            plt.Legend();
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

            var users = (from r in _records
                        group r by r.User
                        into lst
                        select new
                        {
                            User = lst.Key
                        }).Select(u => u.User).ToArray();
            var datapoints = new double[users.Length][];
            for(int i = 0; i < users.Length; i++)
            {
                datapoints[i] = new double[GetTotalWeekCount()];
                for(int j = 0; j < GetTotalWeekCount(); j++)
                {
                    // todo: i have no idea if thats correct...
                    datapoints[i][j] = perUser.Where(pu => pu.User == users[i]).Sum(wks => wks.Weeks.Where(w => w.Week == j + 1).Sum(rs => rs.Records.Sum(r => r.Record.Time)));
                }
            }

            plt.PlotBarGroups(
                Enumerable.Range(1, GetTotalWeekCount()).Select((i) => $"Week {i}").ToArray(),
                users,
                datapoints
                );
            plt.Legend();
        }

        #endregion Graphing

        #region HelperFunctions

        private void CreateGraph(Action<Plot> plot, string name)
        {
            var plt = new ScottPlot.Plot(_options.PlotWidth, _options.PlotHeight);

            plot(plt);
            plt.Title(ChartTitles[name]);

            string path = $"./{_options.OutputDirectory}/{name}.png";
            plt.SaveFig(path);
            Console.WriteLine($"Created {name}.png -> {path}");
        }

        private double Round(double d)
        {
            return Math.Round(d, _options.RoundToDecimals);
        }

        private int WeekNrFromDate(DateTime date)
        {
            DateTime runner = _options.ProjectStart;
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
