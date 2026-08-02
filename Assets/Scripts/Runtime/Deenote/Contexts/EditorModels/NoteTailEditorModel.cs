namespace Deenote.Contexts.EditorModels
{
    internal sealed class NoteTailEditorModel : IStageDisplayNoteEditorModel
    {
        public uint Uid { get; } = INoteUnique.NextUid();

        private NoteEditorModel _head;

        public NoteEditorModel Head => _head;

        public float Time => _head.Time + _head.ActualDuration;

        internal NoteTailEditorModel(NoteEditorModel head)
        {
            _head = head;
        }
    }
}
