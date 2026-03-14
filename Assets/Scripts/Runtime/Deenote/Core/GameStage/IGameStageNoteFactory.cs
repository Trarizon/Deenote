#nullable enable

using Deenote.Core.EditorModels;
using Deenote.Library;
using UnityEngine.Pool;

namespace Deenote.Core.GameStage
{
    internal interface IGameStageNoteFactory
    {
        GameStageNoteController Create(NoteEditorModel note);
        void Return(GameStageNoteController note);
    }

    internal sealed class DeemoGameStageNoteFactory : IGameStageNoteFactory
    {
        private readonly ObjectPool<GameStageNoteController> _pool;

        internal DeemoGameStageNoteFactory(GameStageNoteController prefab, GameStageNotePlaneController plane)
        {
            _pool = UnityUtils.CreateObjectPool(prefab, plane.ContentTransform);
        }

        public GameStageNoteController Create(NoteEditorModel note) => throw new System.NotImplementedException();
        public void Return(GameStageNoteController note) => throw new System.NotImplementedException();
    }
}
