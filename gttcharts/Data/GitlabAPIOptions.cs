using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts.Data
{
    public class GitlabAPIOptions
    {
        public string ApiUrl { get; set; }
        public string Token { get; set; }
        public string Project { get; set; }
        public int HoursPerDay { get; set; } = 8;
        public int DayPerWeek { get; set; } = 5;
        public int WeeksPerMonth { get; set; } = 4;
    }
}
