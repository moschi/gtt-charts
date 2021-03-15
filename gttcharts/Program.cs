using gttcharts.Charting;
using gttcharts.Data;
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
        static GitlabAPIOptions GitlabAPIOptions;
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            var consumer = new GitlabAPIConsumer(GitlabAPIOptions);
            var data = await consumer.GetData();
            if (data.success)
            {
                var chartBuilder = new GttChartBuilder(Options, data.issues, data.records);
                chartBuilder.RunAll();
                StyledConsoleWriter.WriteInfo("Finished!");
                await host.StopAsync();
            }
            else
            {
                StyledConsoleWriter.WriteInfo("Error when getting data from gitlab.");
                StyledConsoleWriter.WriteInfo("Stopping...");
                await host.StopAsync();
            }

            return;
        }

        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();

                IHostEnvironment env = hostingContext.HostingEnvironment;

                configuration
                    .AddJsonFile("gttchartsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"gttchartsettings.{env.EnvironmentName}.json", true, true)
                    .AddCommandLine(args, new Dictionary<string, string>
                    {
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

                        { "-tk", $"{nameof(GitlabAPIOptions)}:{nameof(GitlabAPIOptions.Token)}" },
                        { "--token", $"{nameof(GitlabAPIOptions)}:{nameof(GitlabAPIOptions.Token)}" },

                        { "-api", $"{nameof(GitlabAPIOptions)}:{nameof(GitlabAPIOptions.ApiUrl)}" },
                        { "--apiurl", $"{nameof(GitlabAPIOptions)}:{nameof(GitlabAPIOptions.ApiUrl)}" },

                        { "-prj", $"{nameof(GitlabAPIOptions)}:{nameof(GitlabAPIOptions.Project)}" },
                        { "--project", $"{nameof(GitlabAPIOptions)}:{nameof(GitlabAPIOptions.Project)}" },
                    });

                IConfigurationRoot configurationRoot = configuration.Build();

                GttChartsOptions options = new();
                configurationRoot.GetSection(nameof(GttChartsOptions))
                                 .Bind(options);
                options.AfterInit();
                Options = options;

                GitlabAPIOptions gitlabAPIOptions = new();
                configurationRoot.GetSection(nameof(GitlabAPIOptions))
                                .Bind(gitlabAPIOptions);
                GitlabAPIOptions = gitlabAPIOptions;
            });
    }
}