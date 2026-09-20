using Deenote.Core.GameStage;
using Deenote.GameStage.World;
using Deenote.Library;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    internal interface IGameStageNoteFactory
    {
        GameStageNoteController Create();
        void Release(GameStageNoteController controller);
    }

    internal sealed class DefaultGameStageNoteFactory : IGameStageNoteFactory
    {
        private readonly GameStageNoteController _prefab;
        private readonly GameStageNotePlaneController _plane;
        private readonly ObjectPool<GameStageNoteController> _pool;

        public DefaultGameStageNoteFactory(
            GameStageNoteController prefab, GameStageNotePlaneController plane)
        {
            _prefab = prefab;
            _plane = plane;
            _pool = UnityUtils.CreateObjectPool(() =>
            {
                var item=Object.Instantiate(_prefab,_plane.ContentTransform);
                // item.OnInstantiate(_conte);
                return item;
            });
        }

        public GameStageNoteController Create()
        {
            return _pool.Get();
        }

        public void Release(GameStageNoteController controller)
        {
            _pool.Release(controller);
        }
    }
}
