using Deenote.Core.Editing;
using Deenote.Core.GameStage;
using Deenote.GameStage.World;
using Deenote.Library;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    internal interface IGameStageNoteFactory : System.IDisposable
    {
        GameStageNoteController Create();
        void Release(GameStageNoteController controller);
        GameStageSpeedChangeWarningNoteController CreateWarningNote();
        void Release(GameStageSpeedChangeWarningNoteController controller);
        PlacementNoteIndicatorController CreateIndicator();
        void Release(PlacementNoteIndicatorController controller);
    }

    internal sealed class DefaultGameStageNoteFactory : IGameStageNoteFactory
    {
        private readonly GameStageConfig _config;
        private readonly GameStageController _stage;
        private readonly ObjectPool<GameStageNoteController> _pool;
        private readonly ObjectPool<GameStageSpeedChangeWarningNoteController> _warningNotePool;
        private readonly ObjectPool<PlacementNoteIndicatorController> _indicatorPool;

        public DefaultGameStageNoteFactory(GameStageConfig config, GameStageController stage)
        {
            _config = config;
            _stage = stage;
            _pool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_config.NotePrefab, _stage.NotePlane.ContentTransform);
                item.OnInstantiate(stage);
                return item;
            });
            _warningNotePool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_config.WarningNotePrefab, _stage.NotePlane.ContentTransform);
                item.OnInstantiate(stage);
                return item;
            });
            _indicatorPool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_config.NoteIndicatorPrefab, _stage.IndicatorsPlane.ContentTransform);
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

        public PlacementNoteIndicatorController CreateIndicator()
        {
            return _indicatorPool.Get();
        }

        public void Release(PlacementNoteIndicatorController controller)
        {
            _indicatorPool.Release(controller);
        }

        public void Dispose()
        {
            _pool.Dispose();
            _warningNotePool.Dispose();
            _indicatorPool.Dispose();
        }
    }
}
