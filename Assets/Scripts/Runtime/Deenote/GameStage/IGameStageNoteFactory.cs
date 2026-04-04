#nullable enable

using Deenote.Core.GameStage;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    internal interface IGameStageNoteFactory
    {
        GameStageNoteController Create();
        void Return(GameStageNoteController note);
    }

    internal class DefaultGameStageNoteFactory : IGameStageNoteFactory
    {
        private readonly GameStageNotePlaneController _plane;
        private readonly GameStageNoteController _prefab;
        private readonly ObjectPool<GameStageNoteController> _pool;

        public DefaultGameStageNoteFactory(GameStageNoteController prefab, GameStageNotePlaneController plane)
        {
            _prefab = prefab;
            _plane = plane;
            _pool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_prefab, _plane.ContentTransform);
                item.OnInstantiate(_plane);
                return item;
            });
        }

        public GameStageNoteController Create()
        {
            var item = _pool.Get();
            return item;
        }

        public void Return(GameStageNoteController note)
        {
            _pool.Release(note);
        }
    }
}
