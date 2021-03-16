# Configuration options

gttcharts allows extensive configuration. A example of a configuration file can be found [here](./example/gttchartsettings.json). The configuration file named *gttchartsettings.json* needs to be in the current working directory, and all paths are relative to the current working directory.

There are three sections in the configuration:

- GitlabAPIOptions
- GttChartsOptions



## GitlabAPIOptions

These settings are set for information on how to query the Gitlab API. A minimal setup looks like this:

```
  "GitlabAPIOptions": {
    "ApiUrl": "http://gitlab.com/api/v4/",
    "Token": "abcdefghijklmnopqrst",
    "Project": "namespace/projectname"
  }
```



### ApiUrl

The URL to the gitlab API you want to query.

*required*

```json
	"ApiUrl": "http://gitlab.com/api/v4/",
```



### Token

Gitlab API token used to query the API

*required*

```json
	"Token": "abcdefghijklmnopqrst",
```



### Project

Path to the project

*required*

```json
	"Project": "namespace/projectname",
```



### HoursPerDay

How many hours one day of work has for you

**Default:** 8

```json
	"HoursPerDay": 8,
```



### DayPerWeek

How many days one week of work has for you

**Default:** 5

```json
	"DayPerWeek": 5,
```



### WeeksPerMonth

How many weeks one month of work has for you

**Default:** 4

```json
	"WeeksPerMonth": 5,
```



## GttChartsOptions

### IgnoreEmptyIssues

If true, issues with no time estimate will not be included in charts where issues are a dimension.

**Default:** true

```json
	"IgnoreEmptyIssues": true,
```



### IgnoreLabels

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



### IgnoreMilestones

A list of Milestone names that should be excluded from charts in which the Milestone of an issue is a dimension. When creating a time report during a project where all milestones are already defined, unstarted milestones can excluded from reports with this option.

**Default:** empty

```json
    "IgnoreMilestones": [
      "Alpha",
      "Architecture",
      ""
    ],
```



### IgnoreUsers

A list of User-names that should be excluded from charts in which the User of a record are a dimension. 

**Default:** empty

```json
    "IgnoreUsers": [
      "john.doe"
    ],
```



### OutputDirectory

Path to the directory in which all output will be created.

**Default:** output

```json
    "OutputDirectory": "output",
```



### CreateMarkdownOutput

Specifies whether a markdown file containing all images should be created.

**Default:** true

```json
	"CreateMarkdownOutput": true,
```



### MarkdownOutputName

The name of the markdown file that is to be created. This option only has an effect if ``CreateMarkdownOutput`` is set to ``true``.

**Default:** Timereport

```json
	"MarkdownOutputName": "Timereport",
```



### MarkdownAssetFolder

Specifies whether a folder should be created in which all created charts (images) are stored. This option only has an effect if ``CreateMarkdownOutput`` is set to ``true``. The name of the asset folder will be ``MarkdownOutputName``.assets, e.g. ``Timereport.assets``.

**Default:** true

```json
	"MarkdownAssetFolder": true,
```



### DefaultPlotHeight

Specifies the default height (in pixels) a chart should have. This is used as a fallback value when not specifying a height for a specific plot in the chart-job options.

**Default:** 600

```json
	"DefaultPlotHeight": 600,
```



### DefaultPlotWidth

Specifies the default width (in pixels) a chart should have. This is used as a fallback value when not specifying a width for a specific plot in the chart-job options.

**Default:** 800

```json
	"DefaultPlotWidth": 800,
```



### DefaultYScaleWidth

Specifies the width (in pixels) the scale of the Y-axis will be. This is used as a fallback value when not specifying a YScaleWidth for a specific plot in the chart-job options.

**Default:** 20

```json
	"DefaultYScaleWidth": 20,
```



### DefaultXScaleHeight

Specifies the width (in pixels) the scale of the X-axis will be. This is used as a fallback value when not specifying a XScaleHeight for a specific plot in the chart-job options.

**Default:** 20

```json
	"DefaultXScaleHeight": 20,
```



### RoundToDecimals

Specifies the number of decimal points each numeric value should be rounded to.

**Default:** 2

```json
	"RoundToDecimals": 2,
```



### ProjectStart

Specifies the date at which the project starts. This values is needed to calculate data for charts in which the week number is a dimension.

**Default:** 22.02.2021

```json
	"ProjectStart": "2021-02-22"
```



### ProjectEnd

Specifies the date at which the project ends. This values is needed to calculate the total number of weeks which in turn is needed to create correct scale ticks.

**Default:** 10.06.2021

```json
	"ProjectEnd": "2021-06-10"
```



### UsernameMapping

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



### GttChartJobOptions

This dictionary allows for the definition of chart-job specific options. A complete list of all chart-jobs and their default titles is found further down in the document. The following paragraph explains the options possible for each chart-job. A example of these settings is found at the bottom of this section.



#### Create

Specifies whether a chart-job should be run and included in the final result.

**Default:** true



#### Title

Specifies the title a chart-job produces on the image and in the markdown.



#### Filename

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



#### XScaleHeight

Specifies the height (in pixels) the X-axis ot the chart produced by this job will have. This options is important for charts that have long names on the scales of the axis (e.g. the PerIssue chart).

**Default:** Value specified in ``DefaultXScaleHeight``



#### XLabel

Specifies the label the X-axis of the chart produced by this job will have. Set to ``null`` to hide the label.



#### YLabel

Specifies the label the Y-axis of the chart produced by this job will have. Set to ``null`` to hide the label.



### Complete example

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



## Command Line arguments

Some configuration can also be done trough command line arguments. Command line arguments override the configuration values in gttchartsettings.json.
The following table gives an overview for arguments in the **GttChartsOptions** section:

| Name                 | Short key | Alias key           | Expects      |
| -------------------- | --------- | ------------------- | ------------ |
| IgnoreEmptyIssues    | -ie       | --ignoreempty       | true / false |
| OutputDirectory      | -o        | --output            | string       |
| CreateMarkdownOutput | -md       | --createmarkdown    | true / false |
| MarkdownOutputName   | -mdout    | --markdownoutput    | string       |
| DefaultPlotHeight    | -dph      | --defaultplotheight | integer      |
| DefaultPlotWidth     | -dpw      | --defaultplotwidth  | integer      |
| RoundToDecimals      | -r        | --roundtodecimals   | integer      |

The following table gives an overview for arguments in the **GitlabAPIOptions** section:

| Name    | Short key | Alias key | Expects              |
| ------- | --------- | --------- | -------------------- |
| Token   | -tk       | --token   | your API Token       |
| ApiUrl  | -api      | --apiurl  | URL to the API       |
| Project | -prj      | --project | Path to your project |



## List of chart-jobs and their titles

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

