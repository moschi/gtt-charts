using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts
{
    public class GttChartsOptions
    {
        public string DatabasePath { get; set; } = "data.db";
        public bool IgnoreEmptyIssues { get; set; } = true;
        public ICollection<string> IgnoreLabels { get; set; } = new List<string>();
        public ICollection<string> IgnoreMilestones { get; set; } = new List<string>();
        public ICollection<string> IgnoreUsers { get; set; } = new List<string>();
        public string OutputDirectory { get; set; }
        public bool CreateMarkdownOutput { get; set; } = true;
        public string MarkdownOutputName { get; set; } = "Timereport";
        public bool MarkdownAssetFolder { get; set; } = true;
        public int PlotHeight { get; set; } = 600;
        public int PlotWidth { get; set; } = 800;
        public int RoundToDecimals { get; set; } = 2;
        public DateTime ProjectStart { get; set; } = new DateTime(2021, 02, 22);
        public DateTime ProjectEnd { get; set; } = new DateTime(2021, 06, 10); // todo: fix date
    }
}
