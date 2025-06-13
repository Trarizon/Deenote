#nullable enable

using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class SpeedChangeWarningModel : IStageTimeNode, IStageSelectableNode
    {
        private readonly NoteModel _noteModel;
        private IStageSelectableNode.SelectionProvider _selectionProvider;

        public float Time => _noteModel.Time;

        public bool IsSelected
        {
            get => _selectionProvider.IsSelected;
            set => _selectionProvider.IsSelected = value;
        }

        internal SpeedChangeWarningModel(NoteModel note)
        {
            Debug.Assert(!note.IsVisibleOnStage());
            Debug.Assert(!note.HasSounds);
            Debug.Assert(note.WarningType is WarningType.SpeedChange);
            _noteModel = note;
        }

        public SpeedChangeWarningModel(float time) :
            this(new NoteModel() { Time = time, Position = 4f, WarningType = WarningType.SpeedChange })
        { }

        public SpeedChangeWarningModel Clone()
            => new(_noteModel.Clone(cloneSounds: false));

        void IStageSelectableNode.SetIsInSelectionRange(bool value) => _selectionProvider.SetIsInSelectionRange(value);
        void IStageSelectableNode.ApplySelection() => _selectionProvider.ApplySelection();
    }
}