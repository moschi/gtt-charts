using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public ICollection<string> LabelList
        {
            get => Labels?.Split(',');
        }
        public string Milestone { get; set; }

        public string State { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Closed { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
