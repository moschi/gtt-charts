using gttcharts.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ScottPlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gttcharts
{
    class Program
    {
        // todo: remove
        static GttChartsOptions Options;
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            var chartBuilder = new GttChartsBuilder(Options);
            chartBuilder.RunAll();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();

                IHostEnvironment env = hostingContext.HostingEnvironment;

                configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);


                //.AddCommandLine(args, new Dictionary<string, string>
                //    {
                //        { "-db", "DatabasePath" },
                //        { "--database", "DatabasePath" },
                //        { "--ignoreempty", "IgnoreEmptyIssues" },
                //        { "--output", "OutputDirectory" },
                //        { "-o", "OutputDirectory" },
                //        { "--createmarkdown", "CreateMarkdownOutput" },
                //        { "-md", "CreateMarkdownOutput" }
                //    });

                IConfigurationRoot configurationRoot = configuration.Build();

                GttChartsOptions options = new();
                configurationRoot.GetSection(nameof(GttChartsOptions))
                                 .Bind(options);

                Options = options;

                Console.WriteLine($"GttChartsOptions.IgnoreEmptyIssues={options.IgnoreEmptyIssues}");
                Console.WriteLine($"GttChartsOptions.DatabasePath={options.DatabasePath}");
                Console.WriteLine($"GttChartsOptions.DisplayIssueLabels={options.DisplayIssueLabels.Aggregate((a, b) => ($"{a},{b}"))}");
                Console.WriteLine($"GttChartsOptions.IgnoreMilestones={options.IgnoreMilestones?.Aggregate((a, b) => ($"{a},{b}"))}");
                Console.WriteLine($"GttChartsOptions.OutputDirectory={options.OutputDirectory}");

            });
    }
}