#nullable enable

using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.Editing.EditorModels;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Deenote.Contexts
{
    public sealed partial class ProjectContext : INotifyPropertyChanged<ProjectContext>, INotifyPropertyChanging<ProjectContext>
    {
        // REFACTOR: ProjectManager还剩CurrentProject没迁移
        private ProjectEditorModel? _currentProject_bf;
        public ProjectEditorModel? CurrentProject
        {
            get => _currentProject_bf;
            set {
                if (_currentProject_bf == value)
                    return;

                if (value is not null)
                    value.PropertyChanged -= NotifyChanged_Project;
                PropertyChanging?.Invoke(this, new(nameof(CurrentProject)));

                _currentProject_bf = value;
                if (value is not null)
                    value.PropertyChanged += NotifyChanged_Project;
                NotifyChanged_Project(value, PropertyEventArgs.AllProperties);
                PropertyChanged?.Invoke(this, new(nameof(CurrentProject)));
            }
        }

        private ChartEditorModel? _currentChart_bf;
        public ChartEditorModel? CurrentChart
        {
            get => _currentChart_bf;
            set {
                if (_currentChart_bf == value)
                    return;
                PropertyChanging?.Invoke(this, new(nameof(CurrentChart)));
                _currentChart_bf = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentChart)));
            }
        }

        public event Action<ProjectContext, PropertyEventArgs>? PropertyChanged;
        public event Action<ProjectContext, PropertyEventArgs>? PropertyChanging;

        public ProjectContext()
        {
        }

        private void NotifyChanged_Project(ProjectEditorModel? s, PropertyEventArgs e)
        {
            if (e.MatchProperty(nameof(s.Charts))) {
                Update_Project_Charts(s);
            }
        }

        private void Update_Project_Charts(ProjectEditorModel? s)
        {
            if (s is null) {
                CurrentChart = null;
                return;
            }
            // Simple check, if current chart is removed from project, unload this and load the first chart
            if (CurrentChart is null || !s.Charts.Contains(CurrentChart)) {
                CurrentChart = s.Charts.FirstOrDefault();
            }
        }
    
        [Conditional("DEBUG")]
        [MemberNotNull(nameof(CurrentProject))]
        public void AssertProjectLoaded(string message = "Project is not loaded.")
        {
            if (CurrentProject is null)
                throw new InvalidOperationException(message);
        }
    }
}
