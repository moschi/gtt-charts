# gtt-charts

gtt-charts is a small cli application written in .NET 5 which allows the automatic creation of graphs, optionally included in a markdown file, of time tracking in gitlab.

It requires [GitHub - kriskbx/gitlab-time-tracker: 🦊🕘 A command line interface for GitLab's time tracking feature](https://github.com/kriskbx/gitlab-time-tracker) to get the time tracking data from gitlab and builds on the scripts provided in [Samuel / GitLabTimeTrackingTutorial · GitLab (hsr.ch)](https://gitlab.dev.ifs.hsr.ch/murthy10/GitLabTimeTrackingTutorial). Said scripts were altered slightly to include Labels and Milestones in the issue table.

The charts are generated using the awesome [ScottPlot/ScottPlot: Interactive Plotting Library for .NET (github.com)](https://github.com/ScottPlot/ScottPlot) library.

The exported SQLite database is then loaded by gtt-charts and processed into a number of charts which are listed further down.

A complete example for a resulting markdown file can be found [here](./example/Timereport.md).

## Requirements

- .NET 5 for gtt-charts
- Python 3 for running the scripts to create the SQLite DB
- Node.JS for running gitlab-time-tracker



## Examples of charts

![image-20210310113603174](README.assets/image-20210310113603174.png)

![image-20210310113621041](README.assets/image-20210310113621041.png)

![image-20210310113639147](README.assets/image-20210310113639147.png)

## Configuration options

gttcharts allows extensive configuration. A example of a configuration file can be found [here](./example/appsettings.json). The configuration file named *appsettings.json* needs to be in the same directory as the executable.

### List of configurations options

#### DatabasePath

Path to the SQLite database file.

**Default:** data.db

```json
	"DatabasePath": "./path/to/database.db",
```



#### IgnoreEmptyIssues

If true, issues with no time estimate will not be included in charts where issues are a dimension.

**Default:** true

```json
	"IgnoreEmptyIssues": true,
```



#### IgnoreLabels

A list of labels that should be excluded from charts in which the label(s) of an issue are a dimension. Board specific labels (e.g. To Do, Doing) can be excluded from reports with this option.

**Default:** empty

```json
    "IgnoreLabels": [
      "To Do",
      "Doing",
      "In Review",
      "Epic",
      ""
    ],
```



#### IgnoreMilestones

A list of Milestone names that should be excluded from charts in which the Milestone of an issue is a dimension. When creating a time report during a project where all milestones are already defined, unstarted milestones can excluded from reports with this option.

**Default:** empty

```json
    "IgnoreMilestones": [
      "Alpha",
      "Architecture",
      ""
    ],
```



#### IgnoreUsers

A list of User-names that should be excluded from charts in which the User of a record are a dimension. 

**Default:** empty

```json
    "IgnoreUsers": [
      "john.doe"
    ],
```



#### OutputDirectory

Path to the directory in which all output will be created.

**Default:** output

```json
    "OutputDirectory": "output",
```



#### CreateMarkdownOutput

Specifies whether a markdown file containing all images should be created.

**Default:** true

```json
	"CreateMarkdownOutput": true,
```



#### MarkdownOutputName

The name of the markdown file that is to be created. This option only has an effect if ``CreateMarkdownOutput`` is set to ``true``.

**Default:** Timereport

```json
	"MarkdownOutputName": "Timereport",
```



#### MarkdownAssetFolder

Specifies whether a folder should be created in which all created charts (images) are stored. This option only has an effect if ``CreateMarkdownOutput`` is set to ``true``. The name of the asset folder will be ``MarkdownOutputName``.assets, e.g. ``Timereport.assets``.

**Default:** true

```json
	"MarkdownAssetFolder": true,
```



#### DefaultPlotHeight

Specifies the default height (in pixels) a chart should have. This is used as a fallback value when not specifying a height for a specific plot in the chart-job options.

**Default:** 600

```json
	"DefaultPlotHeight": 600,
```



#### DefaultPlotWidth

Specifies the default width (in pixels) a chart should have. This is used as a fallback value when not specifying a width for a specific plot in the chart-job options.

**Default:** 800

```json
	"DefaultPlotWidth": 800,
```



#### DefaultYScaleWidth

Specifies the width (in pixels) the scale of the Y-axis will be. This is used as a fallback value when not specifying a YScaleWidth for a specific plot in the chart-job options.

**Default:** 20

```json
	"DefaultYScaleWidth": 20,
```



#### DefaultXScaleHeight

Specifies the width (in pixels) the scale of the X-axis will be. This is used as a fallback value when not specifying a XScaleHeight for a specific plot in the chart-job options.

**Default:** 20

```json
	"DefaultXScaleHeight": 20,
```



#### RoundToDecimals

Specifies the number of decimal points each numeric value should be rounded to.

**Default:** 2

```json
	"RoundToDecimals": 2,
```



#### ProjectStart

Specifies the date at which the project starts. This values is needed to calculate data for charts in which the week number is a dimension.

**Default:** 22.02.2021

```json
	"ProjectStart": "2021-02-22"
```



#### ProjectEnd

Specifies the date at which the project ends. This values is needed to calculate the total number of weeks which in turn is needed to create correct scale ticks.

**Default:** 10.06.2021

```json
	"ProjectEnd": "2021-06-10"
```



#### UsernameMapping

A dictionary which allows to map gitlab usernames to names of project members. 

**Default:** empty

```json
    "UsernameMapping": {
      "jane.doe": "Jane Doe",
      "john.doe": "John Doe",
      "alan.touring": "Alan Touring",
      "konrad.zuse": "Konrad Zuse",
      "ada.lovelace": "Ada Lovelace"
    },
```



#### GttChartJobOptions

This dictionary allows for the definition of chart-job specific options. A complete list of all chart-jobs and their default titles is found further down in the document. The following paragraph explains the options possible for each chart-job. A example of these settings is found at the bottom of this section.



##### Create

Specifies whether a chart-job should be run and included in the final result.

**Default:** true



##### Title

Specifies the title a chart-job produces on the image and in the markdown.



##### Filename

Specifies the filename for the image a chart-job will produce. Don't include a file extension in this option.



#### PlotHeight

Specifies the height (in pixels) the chart produced by this job will have.

**Default:** Value specified in ``DefaultPlotHeight``



#### PlotWidth

Specifies the width (in pixels) the chart produced by this job will have.

**Default:** Value specified in ``DefaultPlotWidth``



#### YScaleWidth

Specifies the width (in pixels) the Y-axis of the chart produced by this job will have. This options is important for charts that have long names on the scales of the axis (e.g. the PerIssue chart).

**Default:** Value specified in ``DefaultYScaleWidth``



#### GttChartJobOptions.XScaleHeight

Specifies the height (in pixels) the X-axis ot the chart produced by this job will have. This options is important for charts that have long names on the scales of the axis (e.g. the PerIssue chart).

**Default:** Value specified in ``DefaultXScaleHeight``



#### GttChartJobOptions.XLabel

Specifies the label the X-axis of the chart produced by this job will have. Set to ``null`` to hide the label.



#### GttChartJobOptions.YLabel

Specifies the label the Y-axis of the chart produced by this job will have. Set to ``null`` to hide the label.



#### Complete example

```json
    "GttChartJobOptions": {
      "PerIssue": {
        "Title": "Time spent per Issue [hours]",
        "Filename": "timeSpentPerIssue",
        "PlotHeight": 1080,
        "PlotWidth": 1920,
        "YScaleWidth": 180,
        "XScaleHeight": 200,
        "YLabel": "Time in hours",
        "XLabel": "Title of the issue"
      },
      "PerLabelBar": {
        "Create": false
      }
    }
```



### List of chart-jobs and their titles

The **chart-job key** is what you need to use as the property-name under ``GttChartJobOptions`` to set the options for that specific job.

| chart-job key      | title                                                  |
| ------------------ | ------------------------------------------------------ |
| PerIssue           | Time per Issue [hours]                                 |
| PerMilestone       | Time per Milestone [hours], estimate vs. recorded time |
| PerUser            | Time per User [hours]                                  |
| PerUserPerWeekArea | Time per User per Week (Area)                          |
| PerUserPerWeekBar  | Time per User per Week (Bar)                           |
| PerLabelBar        | Time per Label (Bar), estimate vs. recorded time       |
| PerLabelPie        | Time per Label (Pie) [hours]                           |
| UserPerMilestone   | Time per User per Milestone (Bar) [hours]              |
| MilestonePerUser   | Time per Milestone per User (Bar) [hours]              |



## Building gttcharts

Clone this repo and go into the gttcharts folder which contains gttcharts.csproj

To build gttcharts, run the following command:

```powershell
dotnet build .\gttcharts.csproj
```

The created binaries are located in the default folder. In said folder, place appsettings.json that you changed according to your needs.

### Using gttcharts

1. run a .csv export in *gitlab-time-tracker*
   include all the following columns to create a valid table of issues:

   - iid
   - title
   - spent
   - total_estimate
   - labels
   - milestone
- state
   - created_at
- closed
   - updated_at
   
   also, make sure you include closed issues with the ``--closed`` flag

   the command I use looks like this:

   ```powershell
   gtt report --output=csv --issue_columns=iid --issue_columns=title --issue_columns=spent --issue_columns=total_estimate --issue_columns=labels --issue_columns=milestone --issue_columns=state --issue_columns=created_at --issue_columns=closed --issue_columsn=updated_at --closed --file=./scripts/times.csv
   ```
   
   
   
2. run the *SQLite creation scripts* in the script folder

3. run *gttcharts*, make sure you have appropriate appsettings.json