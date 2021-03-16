using System;
using System.Collections.Generic;

#nullable disable

namespace gttcharts.Models
{
    public partial class Record
    {
        public int Iid { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public double Time { get; set; }
        public int NoteIid { get; set; }
        public string NoteBody { get; set; }
    }
}
