#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.Editing.EditorModels;
using Deenote.Entities.Models;
using System;

namespace Deenote.Editing.Contexts
{
    internal sealed partial class ProjectContext : ObservableObject
    {
        [ObservableProperty] ProjectEditorModel? _currentProject;

        public ChartModel? CurrentChart { get; }
        public EditorContext EditorContext { get; }

        public event Action<ChartModel>? ChartNoteCollectionChanged;

        public ProjectContext()
        {
            EditorContext = new EditorContext(this);
        }

        public void RaiseChartNotesChanged(ChartModel? chart)
        {
            if (chart is not null) {
                ChartNoteCollectionChanged?.Invoke(chart);
                return;
            }
            if (CurrentChart is null) {
                return;
            }
            ChartNoteCollectionChanged?.Invoke(CurrentChart);
        }
    }
}
