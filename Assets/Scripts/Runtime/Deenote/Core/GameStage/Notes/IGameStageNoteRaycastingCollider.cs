#nullable enable

namespace Deenote.Core.GameStage.Notes
{
    internal interface IGameStageNoteRaycastingCollider
    {
        IGameStageNoteController NoteController { get; }
    }
}