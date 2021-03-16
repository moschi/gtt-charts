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

## Configuration

Please see the [documentation for configuration](./configuration.md).



## Building gttcharts

Clone this repo

To build gttcharts, run the following command:

```powershell
dotnet build .\gttcharts\gttcharts.csproj
```

### Using gttcharts

Simply execute gttcharts.exe, make sure to have appropriate settings.
