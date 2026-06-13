#nullable enable

using Deenote.CoreB.Notification;
using Deenote.Editing.NotePlacement;
using Deenote.GameStage.Stage;
using System.Collections.Generic;

namespace Deenote.GameStage
{
    internal sealed class GameStagePlacementIndicatorsManager
    {
        private readonly NotePlacementContext _placementContext;
        private IGameStageNoteFactory? _factory;
        private List<PlacementNoteIndicatorController> _indicators = new();

        public GameStagePlacementIndicatorsManager(NotePlacementContext placementContext)
        {
            _placementContext = placementContext;
        }

        public void Initialize(IGameStageNoteFactory factory)
        {
            _factory = factory;

            _placementContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(_placementContext.CurrentPrototypes))) {
                    foreach (var indicator in _indicators) {
                        _factory.Return(indicator);
                    }
                    _indicators.Clear();
                    foreach (var prototype in _placementContext.CurrentPrototypes) {
                        var indicator = _factory.CreateIndicator();
                        indicator.Initialize(prototype);
                        _indicators.Add(indicator);
                    }
                }
            });
        }
    }
}