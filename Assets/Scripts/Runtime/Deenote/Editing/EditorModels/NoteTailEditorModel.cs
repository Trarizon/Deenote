#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    internal sealed partial class NoteTailEditorModel : ObservableObject, IGameStageNoteNode
    {
        public uint Uid { get; set; }

        private NoteEditorModel _head;

        public NoteEditorModel Head => _head;

        public float Position => _head.Position;
        public float Time => _head.EndTime;

        public float Speed => _head.Speed;

        public bool IsComboNode => true;

        public NoteTailEditorModel(NoteEditorModel head)
        {
            Uid = INoteUnique.GetUid();
            head.PropertyChanged += Head_PropertyChanged;
            _head = head;
        }

        private void Head_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(NoteEditorModel.Position):
                case nameof(NoteEditorModel.Time):
                    OnPropertyChanged(e);
                    break;
                case nameof(NoteEditorModel.ActualDuration):
                    OnPropertyChanged("Time");
                    break;
                default:
                    break;
            }
        }
    }
}
