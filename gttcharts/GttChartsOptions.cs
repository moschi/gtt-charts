using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gttcharts
{
    public class GttChartsOptions
    {
        public void AfterInit()
        {
            GttChartJobOptions.AfterInit(DefaultPlotHeight, DefaultPlotWidth, DefaultYScaleWidth, DefaultXScaleHeight);
        }

        public string DatabasePath { get; set; } = "data.db";
        public bool IgnoreEmptyIssues { get; set; } = true;
        public ICollection<string> IgnoreLabels { get; set; } = new List<string>();
        public ICollection<string> IgnoreMilestones { get; set; } = new List<string>() { "Test" };
        public ICollection<string> IgnoreUsers { get; set; } = new List<string>();
        public string OutputDirectory { get; set; } = "output";
        public bool CreateMarkdownOutput { get; set; } = true;
        public string MarkdownOutputName { get; set; } = "Timereport";
        public bool MarkdownAssetFolder { get; set; } = true;

        public int DefaultPlotHeight { get; set; } = 600;
        public int DefaultPlotWidth { get; set; } = 800;

        public int DefaultYScaleWidth { get; set; } = 20;
        public int DefaultXScaleHeight { get; set; } = 20;


        public int RoundToDecimals { get; set; } = 2;
        public DateTime ProjectStart { get; set; } = new DateTime(2021, 02, 22);
        public DateTime ProjectEnd { get; set; } = new DateTime(2021, 06, 10); // todo: fix date
        public Dictionary<string, string> UsernameMapping { get; set; } = new();

        public GttChartJobOptionContainer GttChartJobOptions { get; set; } = new GttChartJobOptionContainer();

        /// <summary>
        /// Returns output directory with trailing slash
        /// </summary>
        /// <returns></returns>
        public string GetOutputPath()
        {
            if (HasOutputPath())
            {
                string outputDirectory = OutputDirectory;
                if (!outputDirectory.EndsWith("/"))
                {
                    outputDirectory += "/";
                }
                return outputDirectory;
            }
            else
            {
                return "./";
            }
        }

        public bool HasOutputPath()
        {
            return OutputDirectory.Trim() != string.Empty && OutputDirectory is not null;
        }
    }

    // originally using a dictionary was planned - but it seems the whole property is overwritten, not allowing for default values.
    // so this class acts as a proxy to make that work
    // todo: do we need to fix this?
    public class GttChartJobOptionContainer : IEnumerable<KeyValuePair<string, GttChartJobOptions>>
    {
        public void AfterInit(int defaultHeight, int defaultWidth, int defaultYScaleWidth, int defaultXScaleHeight)
        {
            // reload plotHeight from Default if they haven't been set through user config
            foreach (var option in InternalDict.Values.Where(v => v.PlotHeight == -1))
            {
                option.PlotHeight = defaultHeight;
            }

            // reload plotWidth from Default
            foreach (var option in InternalDict.Values.Where(v => v.PlotWidth == -1))
            {
                option.PlotWidth = defaultWidth;
            }

            // reload yScaleWidth from Default
            foreach (var option in InternalDict.Values.Where(v => v.YScaleWidth == -1))
            {
                option.YScaleWidth = defaultYScaleWidth;
            }

            // reload xScaleHeight from Default
            foreach (var option in InternalDict.Values.Where(v => v.XScaleHeight == -1))
            {
                option.XScaleHeight = defaultXScaleHeight;
            }
        }

        public GttChartJobOptions this[string key]
        {
            get => GetOption(key);
        }

        private Dictionary<string, GttChartJobOptions> InternalDict = CreateDefaultJobOptions();

        private static Dictionary<string, GttChartJobOptions> CreateDefaultJobOptions()
        {
            var localDict = new Dictionary<string, GttChartJobOptions>();
            void AddJobOption(string name, string title, string xlabel, string ylabel)
            {
                localDict.Add(name, new GttChartJobOptions()
                {
                    Filename = name,
                    PlotHeight = -1,
                    PlotWidth = -1,
                    YScaleWidth = -1,
                    XScaleHeight = -1,
                    Title = title,
                    XLabel = xlabel,
                    YLabel = ylabel
                });
            }

            AddJobOption(nameof(PerIssue), "Time per Issue [hours]", "Issue", "Time");
            AddJobOption(nameof(PerMilestone), "Time per Milestone [hours], estimate vs. recorded time", "Milestone", "Time");
            AddJobOption(nameof(PerUser), "Time per User [hours]", string.Empty, string.Empty);
            AddJobOption(nameof(PerUserPerWeekArea), "Time per User per Week (Area)", "Week", "Cumulated time");
            AddJobOption(nameof(PerUserPerWeekBar), "Time per User per Week (Bar)", "Week", "Time");
            AddJobOption(nameof(PerLabelBar), "Time per Label (Bar), estimate vs. recorded time", "Label", "Time");
            AddJobOption(nameof(PerLabelPie), "Time per Label (Pie) [hours]", string.Empty, string.Empty);
            AddJobOption(nameof(UserPerMilestone), "Time per User per Milestone (Bar) [hours]", "Milestone", "Time");
            AddJobOption(nameof(MilestonePerUser), "Time per Milestone per User (Bar) [hours]", "User", "Time");

            return localDict;
        }

        private GttChartJobOptions GetOption([CallerMemberName] string key = null)
        {
            return InternalDict[key];
        }

        private void SetOption(GttChartJobOptions value, [CallerMemberName] string key = null)
        {
            InternalDict[key] = value;
        }

        public IEnumerator GetEnumerator()
        {
            return InternalDict.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, GttChartJobOptions>> IEnumerable<KeyValuePair<string, GttChartJobOptions>>.GetEnumerator()
        {
            return InternalDict.GetEnumerator();
        }

        public GttChartJobOptions PerIssue
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions PerMilestone
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions PerUser
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions PerUserPerWeekArea
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions PerUserPerWeekBar
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions PerLabelBar
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions PerLabelPie
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions UserPerMilestone
        {
            get => GetOption();
            set => SetOption(value);
        }

        public GttChartJobOptions MilestonePerUser
        {
            get => GetOption();
            set => SetOption(value);
        }
    }

    public class GttChartJobOptions
    {
        // defaults to true
        public bool Create { get; set; } = true;

        // job specific
        public string Title { get; set; }

        // defaults to jobname
        public string Filename { get; set; }

        // defaults to DefaultPlotHeight
        public int PlotHeight { get; set; }

        // defaults to DefaultPlotWidth
        public int PlotWidth { get; set; }

        // defaults to DefaultXScaleHeight
        public int XScaleHeight { get; set; }

        // defaults to DefaultYScaleWidth
        public int YScaleWidth { get; set; }

        // job specific
        public string XLabel { get; set; }

        // job specific
        public string YLabel { get; set; }
    }
}
