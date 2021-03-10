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
            if (chartBuilder.InitSuccessful)
            {
                chartBuilder.RunAll();
                Console.WriteLine("Finished!");
                await host.StopAsync();
                return;
            }
            else
            {
                Console.WriteLine("Chartbuilder did not initialize correctly. Please see above output.");
                Console.WriteLine("Exiting...");
                await host.StopAsync();
                return;
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();

                IHostEnvironment env = hostingContext.HostingEnvironment;

                configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

                // todo: figure out why this isn't working
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
                options.AfterInit();
                Options = options;
            });
    }
}