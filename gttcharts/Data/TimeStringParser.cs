using GitLabApiClient.Models.Notes.Responses;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace gttcharts.Data
{
    // todo: write unit tests for this parser
    public class TimeStringParser
    {
        private readonly GitlabAPIOptions options;

        // todo? should we allow defining these patterns in the config?
        // We might run slower since the Regex cannot be compiled beforehand (should we decide to do that)
        private const string SpentAtGroupName = "spentat";
        private const string SpentGroupName = "spent";
        private static string SpentTimePattern = $"(?:[added][subtracted] )(?<{SpentGroupName}>.*)(?: of time spent at)(?<{SpentAtGroupName}>.*)";

        private const string EstimateGroupName = "estimate";
        private static string EstimateTimePattern = $"(?:changed time estimate to )(?<{EstimateGroupName}>.*)";

        // shamelessly stolen from gtt :)
        // todo: reference this great work in readme
        private const string CompleteKey = "complete";
        private const string SignKey = "sign";
        private const string MonthKey = "months";
        private const string WeekKey = "weeks";
        private const string DayKey = "days";
        private const string HourKey = "hours";
        private const string MinuteKey = "minutes";
        private const string SecondsKey = "seconds";
        private static string[] CaptureGroups =
            {
                MonthKey,
                WeekKey,
                DayKey,
                HourKey,
                MinuteKey,
                SecondsKey
            };

        // todo: possibly break down regex as it is hard to read
        private const string TimePattern = @"^(?:(?<sign>[subtracted])\s*)?(?:(?<months>\d+)mo\s*)?(?:(?<weeks>\d+)w\s*)?(?:(?<days>\d+)d\s*)?(?:(?<hours>\d+)h\s*)?(?:(?<minutes>\d+)m\s*)?(?:(?<seconds>\d+)s\s*)?$";

        private readonly Regex SpentTimeRegex;
        private readonly Regex EstimateTimeRegex;
        private readonly Regex TimeRegex;


        public TimeStringParser(GitlabAPIOptions options)
        {
            this.options = options;
            // todo: should we compile these regexes?
            // slower startup, but faster execution...
            SpentTimeRegex = new Regex(SpentTimePattern, RegexOptions.Compiled);
            EstimateTimeRegex = new Regex(EstimateTimePattern, RegexOptions.Compiled);
            TimeRegex = new Regex(TimePattern);
        }

        public double GetEstimateHours(Note note)
        {
            var match = EstimateTimeRegex.Match(note.Body);
            string estimateTimeStr = match.Groups[EstimateGroupName].Value;

            return ParseTimeString(estimateTimeStr, 1);
        }

        public (DateTime date, double hours) GetSpentHours(Note note)
        {
            var match = SpentTimeRegex.Match(note.Body);
            string spentAtDateStr = match.Groups[SpentAtGroupName].Value;
            string spentTimeStr = match.Groups[SpentGroupName].Value;

            if (!DateTime.TryParse(spentAtDateStr, out DateTime spentAtDate))
            {
                StyledConsoleWriter.WriteError($"Could not parse date: {spentAtDateStr} from note body {note.Body}");
            }

            return (spentAtDate, ParseTimeString(spentTimeStr, note.Body.Contains("subtracted") ? -1 : 1));
        }


        private double ParseTimeString(string timeString, int sign)
        {
            // todo: handle failing parse
            // todo: possibly allow for manually entering time when failing to parse...

            var match = TimeRegex.Match(timeString);
            var valuesDict = new Dictionary<string, int>();

            foreach (var group in CaptureGroups)
            {
                valuesDict.Add(group, match.Groups[group].Success ? int.Parse(match.Groups[group].Value) : 0);
            }

            // since we can't put months into the timespan, they'll have to be represented by days
            // same for weeks
            return GetHoursFromTimespan(new TimeSpan(
                sign * ((valuesDict[MonthKey] * options.WeeksPerMonth * options.DayPerWeek) + (valuesDict[WeekKey] * options.DayPerWeek) + valuesDict[DayKey]),
                sign * valuesDict[HourKey],
                sign * valuesDict[MinuteKey],
                sign * valuesDict[SecondsKey]
                ));
        }

        private double GetHoursFromTimespan(TimeSpan timeSpan)
        {
            return (double)(timeSpan.Days * options.HoursPerDay) + timeSpan.Hours + ((double)timeSpan.Minutes / 60) + ((double)timeSpan.Seconds / 60 / 60);
        }
    }
}
