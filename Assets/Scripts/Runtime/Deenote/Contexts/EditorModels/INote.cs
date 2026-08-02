using Deenote.Models;

namespace Deenote.Contexts.EditorModels
{
    internal interface INoteUnique : INoteTime
    {
        private static uint _uid = 0;
        uint Uid { get; }

        protected static uint NextUid() => unchecked(++_uid);
    }

    internal interface INoteCollision : INoteTime
    {
        float Position { get; }
        int CollisionCount { get; set; }
    }

    internal interface IStageDisplayNoteEditorModel : INoteUnique
    {

    }


}