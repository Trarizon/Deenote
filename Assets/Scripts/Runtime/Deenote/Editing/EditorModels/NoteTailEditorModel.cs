#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    internal sealed partial class NoteTailEditorModel : ObservableObject, IGameStageNode
    {
        private NoteEditorModel _head;

        public float Position => _head.Position;
        public float Time => _head.EndTime;

        public float Speed => _head.Speed;

        public bool IsComboNode => true;

        public NoteTailEditorModel(NoteEditorModel head)
        {
            head.PropertyChanged += Head_PropertyChanged;
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
