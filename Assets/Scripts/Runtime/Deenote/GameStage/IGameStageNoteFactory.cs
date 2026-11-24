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
        GameStageSpeedChangeWarningNoteController CreateWarningNote();
        void Release(GameStageSpeedChangeWarningNoteController controller);
    }

    internal sealed class DefaultGameStageNoteFactory : IGameStageNoteFactory
    {
        private readonly GameStageNoteController _prefab;
        private readonly GameStageSpeedChangeWarningNoteController _warningNotePrefab;
        private readonly GameStageNotePlane _plane;
        private readonly ObjectPool<GameStageNoteController> _pool;
        private readonly ObjectPool<GameStageSpeedChangeWarningNoteController> _warningNotePool;

        public DefaultGameStageNoteFactory(GameStageConfig config, GameStageController stage)
        {
            _prefab = config.NotePrefab;
            _warningNotePrefab = config.WarningNotePrefab;
            _plane = stage.NotePlane;
            _pool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_prefab, _plane.ContentTransform);
                item.OnInstantiate(stage);
                return item;
            });
            _warningNotePool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_warningNotePrefab, _plane.ContentTransform);
                item.OnInstantiate(stage);
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

        public GameStageSpeedChangeWarningNoteController CreateWarningNote()
        {
            return _warningNotePool.Get();
        }

        public void Release(GameStageSpeedChangeWarningNoteController controller)
        {
            _warningNotePool.Release(controller);
        }
    }
}
