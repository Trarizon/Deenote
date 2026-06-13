#nullable enable

using Deenote.GameStage.Stage;
using Deenote.Library;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    internal interface IGameStageNoteFactory
    {
        GameStageNoteController Create();
        void Return(GameStageNoteController note);
        PlacementNoteIndicatorController CreateIndicator();
        void Return(PlacementNoteIndicatorController indicator);
    }

    internal class DefaultGameStageNoteFactory : IGameStageNoteFactory
    {
        private readonly GameStageContext _context;
        private readonly GameStageNotePlaneController _plane;
        private readonly GameStageNoteController _prefab;
        private readonly ObjectPool<GameStageNoteController> _pool;
        private readonly PlacementNotePlaneController _indicatorPlane;
        private readonly PlacementNoteIndicatorController _indicatorPrefab;
        private readonly ObjectPool<PlacementNoteIndicatorController> _indicatorPool;

        public DefaultGameStageNoteFactory(GameStageContext context,
            GameStageNoteController prefab, GameStageNotePlaneController plane,
            PlacementNoteIndicatorController indicatorPrefab, PlacementNotePlaneController indicatorPlane)
        {
            _context = context;

            _prefab = prefab;
            _plane = plane;
            _pool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_prefab, _plane.ContentTransform);
                item.OnInstantiate(_context, _plane);
                return item;
            });
            
            _indicatorPrefab = indicatorPrefab;
            _indicatorPlane = indicatorPlane;
            _indicatorPool = UnityUtils.CreateObjectPool(() =>
            {
                var item = Object.Instantiate(_indicatorPrefab, _indicatorPlane.ContentTransform);
                item.OnInstantiate(_context, _indicatorPlane);
                return item;
            });
        }

        public GameStageNoteController Create()
        {
            var item = _pool.Get();
            return item;
        }

        public PlacementNoteIndicatorController CreateIndicator()
        {
            var item = _indicatorPool.Get();
            return item;
        }

        public void Return(GameStageNoteController note)
        {
            _pool.Release(note);
        }

        public void Return(PlacementNoteIndicatorController indicator)
        {
            _indicatorPool.Release(indicator);
        }
    }
}
