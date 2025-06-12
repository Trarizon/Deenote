#nullable enable

using Deenote.Core.Editing.Indicators;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Library;
using Deenote.Library.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Core.Editing
{
    partial class StageNotePlacer
    {
        private Transform _indicatorPanelTransform = default!;
        private Transform _speedWarningIndicatorPanelTransform = default!;
        private PooledObjectListView<PlacementNoteIndicatorController> _indicators = default!;
        private SpeedChangeWarningIndicatorController _speedChangeWarningIndicator = default!;
        private PlacementArea _indicatorVisibility_bf;

        internal PlacementArea IndicatorVisibility => _indicatorVisibility_bf;

        private void SetIndicatorsVisibility(PlacementArea value)
        {
            if (Utils.SetField(ref _indicatorVisibility_bf, value, out var old)) {
                Debug.Assert(old != value);

                switch (old) {
                    case PlacementArea.NotePlacement:
                        _indicatorPanelTransform.gameObject.SetActive(false);
                        break;
                    case PlacementArea.SpeedChangeWarning:
                        _speedWarningIndicatorPanelTransform.gameObject.SetActive(false);
                        break;
                }

                switch (value) {
                    case PlacementArea.NotePlacement:
                        _indicatorPanelTransform.gameObject.SetActive(true);
                        break;
                    case PlacementArea.SpeedChangeWarning:
                        _speedWarningIndicatorPanelTransform.gameObject.SetActive(true);
                        break;
                }
            }
        }

        private void RefreshIndicatorVisibility()
        {
            if (IsForceShowIndicator() || IsIndicatorOn) {
                SetIndicatorsVisibility(GetPlacementArea(_currentState));
            }
            else {
                SetIndicatorsVisibility(PlacementArea.Invalid);
            }
        }

        [MemberNotNull(
            nameof(_indicatorPanelTransform),
            nameof(_speedWarningIndicatorPanelTransform),
            nameof(_indicators),
            nameof(_speedChangeWarningIndicator))]
        private void ReinitializeIndicators(GamePlayManager.StageLoadedEventArgs args)
        {
            _indicatorPanelTransform = args.Stage.NoteIndicatorPanelTransform;
            _speedWarningIndicatorPanelTransform = args.Stage.SpeedWarningIndicatorPanelTransform;

            _indicators?.Clear();
            var indicators = new PooledObjectListView<PlacementNoteIndicatorController>(
                UnityUtils.CreateObjectPool(args.Stage.Args.PlacementNoteIndicatorPrefab,
                    _indicatorPanelTransform,
                    item => item.OnInstantiate(this)));

            foreach (var note in _prototypes) {
                indicators.Add(out var indicator);
                indicator.Initialize(note);
            }
            _indicators = indicators;

            if (_speedChangeWarningIndicator is not null) {
                _speedChangeWarningIndicator.gameObject.SetActive(false);
                Object.Destroy(_speedChangeWarningIndicator.gameObject);
            }

            _speedChangeWarningIndicator = Object.Instantiate(
                args.Stage.Args.EditorSpeedWarningIndicatorPrefab, _speedWarningIndicatorPanelTransform);
            _speedChangeWarningIndicator.OnInstantiate(this);
        }

        private void MoveNoteIndicatorsTo(NoteCoord coord)
        {
            Debug.Assert(_indicatorPanelTransform.gameObject.activeSelf);
            Debug.Assert(_indicators.Count >= 1);
            if (_indicators.Count == 1) {
                _indicators[0].MoveTo(coord);
                return;
            }

            var baseCoord = coord - _indicators[0].NotePrototype.PositionCoord;
            foreach (var indicator in _indicators) {
                var c = baseCoord + indicator.NotePrototype.PositionCoord;
                c = NoteCoord.Clamp(c, _editor._game.MusicPlayer.ClipLength);
                indicator.MoveTo(c);
            }
        }

        private void MoveSpeedChangeWarningIndicatorTo(float time)
        {
            _speedChangeWarningIndicator.MoveTo(time);
        }
    }
}