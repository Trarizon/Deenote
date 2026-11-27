using Deenote.CoreB.Notification;
using Deenote.CoreB.Notification.Collections;
using Deenote.Entities.Models;
using Deenote.Project;
using System;
using System.Runtime.InteropServices;

namespace Deenote.Editing
{
    public class ChartNotesSelector
    {
        private readonly ObservableList<NoteModel> _selectedNotes = new();
        private readonly ObservableList<SpeedChangeWarningModel> _selectedWarningNotes = new();

        public ObservableList<NoteModel> SelectedNotes => _selectedNotes;
        public ObservableList<SpeedChangeWarningModel> SelectedWarningNotes => _selectedWarningNotes;

        public ChartNotesSelector(ProjectManager2 projectManager)
        {
            projectManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    _selectedNotes.Clear();
                    _selectedWarningNotes.Clear();
                }
            });

            SelectedNotes.CollectionChanged += (s, e) =>
            {
                foreach (var rmv in e.RemovedItems) {
                    rmv.IsSelected = false;
                }
                foreach (var add in e.AddedItems) {
                    add.IsSelected = true;
                }
            };
            SelectedWarningNotes.CollectionChanged += (s, e) =>
            {
                foreach (var rmv in e.RemovedItems) {
                    rmv.IsSelected = false;
                }
                foreach (var add in e.AddedItems) {
                    add.IsSelected = true;
                }
            };
        }

        public void ClearSelection()
        {
            _selectedNotes.Clear();
            _selectedWarningNotes.Clear();
        }

        public void Reselect(ReadOnlySpan<NoteModel> notes, ReadOnlySpan<SpeedChangeWarningModel> warningNotes)
        {
            SelectedNotes.Replace(notes);
            SelectedWarningNotes.Replace(warningNotes);
        }

        public void Reselect(ReadOnlySpan<NoteModel> notes) => Reselect(notes, ReadOnlySpan<SpeedChangeWarningModel>.Empty);
        public void Reselect(NoteModel note) => Reselect(MemoryMarshal.CreateReadOnlySpan(ref note, 1));
        public void Reselect(ReadOnlySpan<SpeedChangeWarningModel> notes) => Reselect(ReadOnlySpan<NoteModel>.Empty, notes);
        public void Reselect(SpeedChangeWarningModel note) => Reselect(MemoryMarshal.CreateReadOnlySpan(ref note, 1));
    }
}