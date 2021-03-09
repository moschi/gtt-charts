using gttcharts.Models;
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
            Console.Read();
        }
    }
}
