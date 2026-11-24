using Deenote.CoreB.Notification;
using Deenote.Entities.Models;
using Deenote.Project;
using ObservableCollections;
using System;

namespace Deenote.Editing
{
    public class ChartNotesSelector
    {
        private readonly ObservableList<NoteModel> _selectedNotes = new();
        private readonly ObservableList<SpeedChangeWarningModel> _selectedWarningNotes = new();

        // TODO: arch1.1: We need custom collection notification to replace these events
        // the collection should provide removed items for Reset
        // Maybe we can use I...Changing to do that
        public event Action<ChartNotesSelector>? SelectedNotesChanging;
        public event Action<ChartNotesSelector>? SelectedNotesChanged;
        public void InvokeChanging()
        {
            // TODO: arch1.1 StageManager手动刷新选中显示，现在Note还没实现INPC
            foreach (var rmv in _selectedNotes) {
                rmv.IsSelected = false;
            }
            foreach (var rmv in _selectedWarningNotes) {
                rmv.IsSelected = false;
            }
            SelectedNotesChanging?.Invoke(this);
        }
        public void InvokeChanged()
        {
            foreach (var add in _selectedNotes) {
                add.IsSelected = true;
            }
            foreach (var add in _selectedWarningNotes) {
                add.IsSelected = true;
            }
            SelectedNotesChanged?.Invoke(this);
        }

        public ChartNotesSelector(ProjectManager2 projectManager)
        {
            projectManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    _selectedNotes.Clear();
                    _selectedWarningNotes.Clear();
                }
            });
        }

        public ObservableList<NoteModel> SelectedNotes => _selectedNotes;
        public ObservableList<SpeedChangeWarningModel> SelectedWarningNotes => _selectedWarningNotes;
    }
}