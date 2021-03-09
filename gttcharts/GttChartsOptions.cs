using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts
{
    public class GttChartsOptions
    {
        public string DatabasePath { get; set; }
        public bool IgnoreEmptyIssues { get; set; } = true;
        public ICollection<string> DisplayIssueLabels { get; set; }
        public ICollection<string> IgnoreMilestones { get; set; }
        public string OutputDirectory { get; set; }
        public string CreateMarkdownOutput { get; set; }
        public int PlotHeight { get; set; } = 1080;
        public int PlotWidth { get; set; } = 1920;
        public int RoundToDecimals { get; set; } = 2;
        public DateTime ProjectStart { get; set; } = new DateTime(2021, 02, 22);
        public DateTime ProjectEnd { get; set; } = new DateTime(2021, 06, 10); // todo: fix date
    }
}
