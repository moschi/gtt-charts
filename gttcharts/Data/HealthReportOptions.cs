using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts.Data
{
    public class HealthReportOptions
    {
        public Dictionary<string, DateTime> IssueHappeningDateMap { get; set; } = new();
        public Dictionary<string, DateTime> IssueMinimumDate { get; set; } = new();
        public Dictionary<string, DateTime> IssueMaximumDate { get; set; } = new();
        public bool CheckZeroDateEntries { get; set; } = true;
    }
}
