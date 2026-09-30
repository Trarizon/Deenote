using Deenote.Core;
using Deenote.Entities;
using Deenote.Entities.Models;
using System;
using System.IO;

namespace Deenote.Project
{
    public interface IChartJsonSaveStrategy
    {
        string GetFileName(ProjectModel project, ChartModel chart, DateTime time);
    }

    internal class ChartJsonSaveStrategy : IChartJsonSaveStrategy
    {
        public static ChartJsonSaveStrategy Default { get; } = new ChartJsonSaveStrategy();

        public string GetFileName(ProjectModel project, ChartModel chart, DateTime time)
        {
            var fileName = Path.GetFileNameWithoutExtension(project.ProjectFilePath);
            var chartName = string.IsNullOrEmpty(chart.Name) ? chart.Difficulty.ToLowerCaseString() : chart.Name;
            return PathUtils.ReplaceInvalidFileNameChars($"{fileName}.{chartName}.{time:yyMMddHHmmss}.json");
        }
    }
}