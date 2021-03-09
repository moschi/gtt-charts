using System;
using System.Collections.Generic;

#nullable disable

namespace gttcharts.Models
{
    public partial class Issue
    {
        public int Iid { get; set; }
        public string Title { get; set; }
        public double Spent { get; set; }
        public double TotalEstimate { get; set; }
        public string Labels { get; set; }
        public string Milestone { get; set; }
    }
}
