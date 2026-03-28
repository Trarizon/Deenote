#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.Editing.EditorModels;
using System;

namespace Deenote.Editing.Contexts
{
    internal sealed partial class ProjectContext : ObservableObject
    {
        [ObservableProperty] ProjectEditorModel? _currentProject;

        public ChartEditorModel? CurrentChart { get; }
        public EditorContext EditorContext { get; }

        public event Action<ChartEditorModel>? ChartNoteCollectionChanged;

        public ProjectContext()
        {
            EditorContext = new EditorContext(this);
        }

        public void RaiseChartNotesChanged(ChartEditorModel? chart)
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
