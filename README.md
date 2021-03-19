# gtt-charts

gtt-charts is a cli application written in .NET 5 which allows the automatic creation of graphs, optionally included in a markdown file, of time tracking in gitlab. It uses the Gitlab API to get information about issues and time tracking entries. Parsing logic was adapted from [kriskbx/gitlab-time-tracker: 🦊🕘 A command line interface for GitLab's time tracking feature. (github.com)](https://github.com/kriskbx/gitlab-time-tracker).

The charts are generated with the awesome [ScottPlot/ScottPlot: Interactive Plotting Library for .NET (github.com)](https://github.com/ScottPlot/ScottPlot) library.

A complete example for a resulting markdown file can be found [here](./example/Timereport.md).



## Disclaimer / Acknowledgements

I cannot guarantee that this software will work in your situation, and I cannot guarantee that the reports generated are free of errors. Please do review the charts critically and compare the numbers with the insights possible in GitLab.

Should you find any bugs, please create an Issue and - preferably - create a PR that fixes the bug you discovered.



## ToDo / Help appreciated

Currently there are is no automated testing in this project. Any pull requests helping with this are greatly appreciated.

Also, this project needs a review of the code, any feedback is appreciated.



## Requirements

- .NET 5



## Examples of charts

![image-20210310113603174](README.assets/image-20210310113603174.png)

![image-20210310113621041](README.assets/image-20210310113621041.png)

![image-20210310113639147](README.assets/image-20210310113639147.png)

## Data Health Report

I've introduced this feature for two reasons:

- We used issues as time-tracking for meetings, sometimes we accidentally added time for those meetings on a wrong date. Charts on which one dimension was time would have had a skewed display of reality
- GitLab sometimes has issues where the timestats of an issue aren't correct. I have not been able to reproduce this, but I wanted to catch these errors.

You can activate the HealthReport by adding the corresponding setting in [GttChartsOptions](./configuration.md#RunHealthReport).

See the configuration for the HealthReport in the [documentation for configuration](./configuration.md#HealthReportOptions).

HealthReport has the ability to check the following things:

- Time spent on an issue isn't on a specific date
- Time spent on an issue isn't after a specific date
- Time spent on an issue isn't before a specific date
- Time spent on an issue has the minimal possible date
- Total Time spent on an issue taken from timestats (the value you see in Gitlab Web-UI) differs from the aggregate of the single records



## Configuration

Please see the [documentation for configuration](./configuration.md).



## Building gttcharts

Clone this repo

To build gttcharts, run the following command:

```powershell
dotnet build .\gttcharts\gttcharts.csproj
```

### Using gttcharts

Simply execute gttcharts (possibly with command line arguments), make sure to have appropriate settings.
