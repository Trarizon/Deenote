#nullable enable

using Deenote;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.GameStage;
using Deenote.GameStage.Grids;
using Deenote.Library;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.Stage
{
    [MovedFrom("Deenote.Core.GameStage")]
    public abstract class PlacementNoteIndicatorController : MonoBehaviour
    {
        private const float NoteAlpha = 0.5f;

        private GameStageContext _stageContext = default!;

        private PlacementNotePlaneController _plane = default!;

        private Vector2 _localPosition;
        protected Vector2? _linkLineEndOffset;
        protected NotePrototypeModel? _note;

        public NotePrototypeModel NotePrototype
        {
            get => _note!;
        }

        public GameStageController GameStage => _plane.GameStage;

        internal void OnInstantiate(GameStageContext stageContext, PlacementNotePlaneController plane)
        {
            _plane = plane;
            _stageContext = stageContext;

            var stage = _plane.GameStage;
            //game.AssertStageLoaded();
            stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal protected abstract void Refresh();

        internal void Initialize(NotePrototypeModel note)
        {
            if (_note is not null) {
                _note.PropertyChanged -= _OnNoteChanged;
            }
            _note = note;
            _note.PropertyChanged += _OnNoteChanged;

            Refresh();
        }

        private void OnDestroy()
        {
            _plane.GameStage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _linkLineEndOffset = null;
        }

        private void _OnNoteChanged(NotePrototypeModel note, PropertyEventArgs e)
        {
            if (e.MatchProperty(nameof(note.PositionCoord))) {
                Update_PositionCoord(note.PositionCoord);
            }
            OnNonCoordPropertyChanged(e);
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            var showLinkLine = _stageContext.IsShowLinkLines;
            if (showLinkLine && _linkLineEndOffset is { } offset) {
                //GameStage.GamePlay.AssertStageLoaded();

                var args = GameStage.ThemeEntry.GridLineConfig;
                //var args = GameStage.GridLineArgs;
                collector.AddLine(_localPosition, _localPosition + offset,
                    args.LinkLineColor with { a = NoteAlpha },
                    args.LinkLineWidth);
            }
        }

        private void Update_PositionCoord(NoteCoord localCoord)
        {
            var (x, z) = _plane.GameStage.ThemeEntry.NoteCoordStrategy.CoordToXZ(localCoord, _stageContext.ActualNoteFallSpeed, NotePrototype.Speed);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);
        }

        protected abstract void OnNonCoordPropertyChanged(PropertyEventArgs e);

        public void MoveTo(NoteCoord coord)
        {
            var localCoord = coord - new NoteCoord(position: 0, time: _stageContext.NotesContext.CurrentTime);
            var (x, z) = GameStage.EvaluateNoteWorldXZ(localCoord, NotePrototype.Speed);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);
        }
    }
}