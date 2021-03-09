using gttcharts.Models;
using ScottPlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace gttcharts
{
    class Program
    {
        static void Main(string[] args)
        {
            ICollection<Issue> issues = null;
            ICollection<Record> records = null;
            using (var context = new GttContext())
            {
                issues = context.Issues.ToList();
                records = context.Records.ToList();
            }

            var query = from i in issues
                        join r in records
                        on i.Iid equals r.Iid
                        into list
                        select new
                        {
                            Issue = i,
                            Records = list
                        };

            foreach (var row in query)
            {
                Console.WriteLine($"Issue: {row.Issue.Title}, estimated {row.Issue.TotalEstimate}h, spent {row.Issue.Spent}");
                var q2 = from r in row.Records
                         group r by r.User
                         into lst
                         select new
                         {
                             User = lst.Key,
                             Spent = lst.Sum(r => r.Time)
                         };
                foreach (var usrTime in q2)
                {
                    Console.WriteLine($"{usrTime.User} spent {usrTime.Spent}h on this issue");
                }
                Console.WriteLine("=========");
            }

            var plt = new ScottPlot.Plot();
            var perMilestone = from i in issues
                               group i by i.Milestone
                               into lst
                               select new
                               {
                                   Milestone = lst.Key,
                                   Estimate = lst.Sum(i => i.TotalEstimate),
                                   Spent = lst.Sum(i => i.Spent)
                               };

            string[] milestonenames = perMilestone.Select(i => i.Milestone).ToArray();
            double[] estimates = perMilestone.Select(p => p.Estimate).ToArray();
            double[] spent = perMilestone.Select(p => p.Spent).ToArray();
            plt.PlotBarGroups(milestonenames, new string[] { "estimated", "spent" }, new double[][] { estimates, spent }, showValues: true);
            plt.Legend(location: legendLocation.upperRight);
            plt.SaveFig("PerMilestone.png");

            Console.Read();
        }
    }
}
