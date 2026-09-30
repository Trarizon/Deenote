using System;

namespace Deenote.Project
{
    partial class ProjectManager2
    {
        public readonly record struct ProjectSaveEventArgs(
            ProjectSaveContents Contents,
            string TargetFilePath
        );

        [Flags]
        public enum ProjectSaveContents
        {
            None = 0,
            Project = 1,
            ChartJsons = 2,
        }

        public readonly record struct ProjectChartJsonsSavingProgress(
            int Saved,
            int Total
        );
    }
}
