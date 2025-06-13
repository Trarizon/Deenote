#nullable enable

namespace Deenote.Entities.Models
{
    public interface IStageTimeNode
    {
        float Time { get; }
    }

    public interface IStageNoteNode : IStageTimeNode
    {
        private static uint s_uid;

        float Speed { get; }
        float Position { get; }

        bool IsComboNode { get; }

        internal uint Uid { get; }

        internal static void InitUid(ref uint uid) => uid = s_uid++;
    }

    public interface IStageSelectableNode
    {
        /// <summary>
        /// Is selected in editor
        /// </summary>
        bool IsSelected { get; set; }
        void SetIsInSelectionRange(bool value);
        void ApplySelection();

        internal struct SelectionProvider
        {
            private bool _isSelected;
            /// <summary>
            /// If editor is selecting notes now, this field indicates whether the note is in selection range 
            /// </summary>
            private bool _isInSelectionRange;

            public bool IsSelected
            {
                get => _isSelected ^ _isInSelectionRange;
                set {
                    _isSelected = value;
                    _isInSelectionRange = false;
                }
            }

            public void SetIsInSelectionRange(bool value) => _isInSelectionRange = value;

            public void ApplySelection()
            {
                _isSelected = IsSelected;
                _isInSelectionRange = false;
            }
        }
    }
}