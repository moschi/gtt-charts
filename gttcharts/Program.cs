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
        // todo: figure out if this is the correct way of handling IConfiguration
        // todo: should we use DI to make the configuration available in the whole project
        static GttChartsOptions Options;
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            var chartBuilder = new GttChartBuilder(Options);
            if (chartBuilder.InitSuccessful)
            {
                chartBuilder.RunAll();
                StyledConsoleWriter.WriteInfo("Finished!");
                await host.StopAsync();
                return;
            }
            else
            {
                StyledConsoleWriter.WriteError("Chartbuilder did not initialize correctly. Please see above output.");
                StyledConsoleWriter.WriteInfo("Exiting...");
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
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                    .AddCommandLine(args, new Dictionary<string, string>
                    {
                        { "-db", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.DatabasePath)}" },
                        { "--database", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.DatabasePath)}" },

                        { "-ie", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.IgnoreEmptyIssues)}" },
                        { "--ignoreempty", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.IgnoreEmptyIssues)}" },

                        { "-o", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.OutputDirectory)}" },
                        { "--output", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.OutputDirectory)}" },

                        { "-md", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.CreateMarkdownOutput)}" },
                        { "--createmarkdown", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.CreateMarkdownOutput)}" },

                        { "-mdout", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.MarkdownOutputName)}" },
                        { "--markdownoutput", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.MarkdownOutputName)}" },

                        { "-dph", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.DefaultPlotHeight)}" },
                        { "--defaultplotheight", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.DefaultPlotHeight)}" },

                        { "-dpw", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.DefaultPlotWidth)}" },
                        { "--defaultplotwidth", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.DefaultPlotWidth)}" },

                        { "-r", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.RoundToDecimals)}" },
                        { "--roundtodecimals", $"{nameof(GttChartsOptions)}:{nameof(GttChartsOptions.RoundToDecimals)}" },
                    });

                IConfigurationRoot configurationRoot = configuration.Build();

                GttChartsOptions options = new();
                configurationRoot.GetSection(nameof(GttChartsOptions))
                                 .Bind(options);
                options.AfterInit();
                Options = options;
            });
    }
}