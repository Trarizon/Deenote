#nullable enable

using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.GameStage;
using Deenote.Library;
using Deenote.CoreB.Unity;
using Deenote.Library.Mathematics;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal abstract class GameStageNoteController : MonoBehaviour
    {
        protected GameStageManager _stage = default!;

        public NoteModel NoteModel { get; private set; } = default!;

        private (Vector2, Vector2)? _linkLine;
        private float _noteColorAlpha;

        [SerializeField]
        private NoteDisplayState _state;

        // Appear ahead time of note when sudden+ is 0,
        // The value may be affected if the note is following a high-speed note
        private float _appearAheadTime0SuddenPlus;
        private float _stageDeltaTime;

        // The actual appear ahead time, the value 
        private float AppearAheadTime
        {
            get {
                _stage.AssertStageLoaded();

                var suddenPlusAheadTime = _stage.GetStageNoteAppearAheadTime(NoteModel.Speed);
                float aheadTime;
                if (_stage.IsEarlyDisplaySlowNotes) {
                    aheadTime = suddenPlusAheadTime;
                }
                else {
                    aheadTime = Mathf.Min(suddenPlusAheadTime, _appearAheadTime0SuddenPlus);
                }
                return aheadTime;
            }
        }

        internal void OnInstantiate(GameStageManager stage)
        {
            _stage = stage;

            _stage.AssertStageLoaded();
            _stage.GameStage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal void Initialize(NoteModel noteModel)
        {
            NoteModel = noteModel;
        }

        internal void PostInitialize(GameStageNoteController? previousStageNote)
        {
            SetAppearAheadTime0SuddenPlus(previousStageNote);
            RefreshVisual();
            RefreshStageDeltaTime();
        }

        private void OnDestroy()
        {
            if (_stage.IsStageLoaded())
                _stage.GameStage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _state = NoteDisplayState.Inactive;
            SetLinkLine();
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (_linkLine is var (start, end)) {
                _stage.AssertStageLoaded();
                collector.AddLine(start, end,
                    _stage.GameStage.GridLineConfig.LinkLineData.ColorWithAlpha(_noteColorAlpha),
                    _stage.GameStage.GridLineConfig.LinkLineData.Width);
            }
        }

        #region Refresh

        /// <summary>
        /// Called when music time updated
        /// </summary>
        private void RefreshTimeDisplayState()
        {
            SetState();
            switch (_state) {
                case NoteDisplayState.Invisible:
                    SetLinkLine();
                    break;
                case NoteDisplayState.Fall:
                    SetNotePositionZ(_stageDeltaTime);
                    SetNoteSpriteAlpha();
                    SetLinkLine();
                    SetHoldBodyDisplayLength();
                    break;
                case NoteDisplayState.Holding:
                    SetNotePositionZ(0f);
                    SetLinkLine();
                    SetHoldBodyDisplayLength();
                    SetHoldingHitEffect();
                    break;
                case NoteDisplayState.HitEffect:
                    SetNotePositionZ(0f);
                    SetLinkLine();
                    SetHitEffect(-_stageDeltaTime - NoteModel.GetActualDuration());
                    break;
            }
        }

        private NoteDisplayState GetState()
        {
            if (IsInvisible())
                return NoteDisplayState.Invisible;
            if (_stageDeltaTime >= 0)
                return NoteDisplayState.Fall;
            if (_stageDeltaTime > -NoteModel.GetActualDuration())
                return NoteDisplayState.Holding;
            return NoteDisplayState.HitEffect;

            bool IsInvisible()
            {
                if (_stageDeltaTime >= AppearAheadTime)
                    return true;

                if (!_stage.IsEarlyDisplaySlowNotes) {
                    // In TimeOrder mode, the note should display only after its previous note displayed
                    if (_stage.NotesManager.GetNextActiveNodeInTimeOrderDisplayMode() is { } next) {
                        if (NodeTimeUniqueComparer.Instance.Compare(NoteModel, next) >= 0) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void RefreshHoldLength()
        {
            SetHoldBodyDisplayLength();
        }

        public void RefreshStageDeltaTime()
        {
            _stageDeltaTime = NoteModel.Time - _stage.MusicTime;
            RefreshTimeDisplayState();
        }

        public void RefreshLinkLine()
        {
            SetLinkLine();
        }

        /// <summary>
        /// Set note's properties according to <see cref="NoteModel"/>, except time
        /// </summary>
        public void RefreshVisual()
        {
            _stage.AssertStageLoaded();

            SetNotePositionX();
            SetNoteSprite();
            SetNoteSize();
            RefreshColoring();
            SetLinkLine();
            RefreshHoldLength();
        }

        public void RefreshColorAlpha()
        {
            if (_state is NoteDisplayState.Invisible or NoteDisplayState.Fall) {
                SetState();
                if (_state is NoteDisplayState.Invisible)
                    SetLinkLine();
                if (_state is NoteDisplayState.Fall)
                    SetNoteSpriteAlpha();
            }
        }

        public void RefreshColoring()
        {
            if (_state is NoteDisplayState.Fall) {
                SetNoteSpriteColor();
            }
        }

        #endregion

        #region Setters

        protected abstract void SetHoldingHitEffect();

        protected abstract void SetHitEffect(float timeAfterHit);

        private void SetNotePositionX()
        {
            transform.WithLocalPositionX(_stage.ConvertNoteCoordPositionToWorldX(NoteModel.Position));
        }

        protected abstract void SetNoteSprite();

        protected abstract void SetNoteSize();

        private void SetNotePositionZ(float time)
        {
            _stage.AssertStageLoaded();

            float z = _stage.ConvertNoteCoordTimeToWorldZ(time, NoteModel.Speed);
            transform.WithLocalPositionZ(z);
        }

        protected void SetNoteSpriteAlpha()
        {
            _stage.AssertStageLoaded();
            Debug.Assert(_state is NoteDisplayState.Fall);

            var appearAheadTime = AppearAheadTime;
            var noteFadeInEndTime = appearAheadTime * (1 - _stage.Config.NoteFadeInRatio);

            var maxAlpha = _stage.IsFilterNoteSpeed && !Mathf.Approximately(NoteModel.Speed, _stage.HighlightedNoteSpeed)
                ? ((DeemoGameStageNoteController)this)._config.DownplayAlpha
                : 1f;
            _noteColorAlpha = MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0, maxAlpha);

            SetNoteSpriteRendererAlpha(_noteColorAlpha);
        }

        protected abstract void SetNoteSpriteRendererAlpha(float alpha);

        private void SetLinkLine()
        {
            _stage.AssertStageLoaded();

            if (_state is NoteDisplayState.Fall && _stage.IsShowLinkLines && NoteModel.NextLink is not null) {
                var currentTime = _stage.MusicTime;

                var to = NoteModel.NextLink;
                var from = NoteModel;

                var (fromX, fromZ) = _stage.ConvertNoteCoordToWorldPosition(from.PositionCoord - new NoteCoord(0f, currentTime), from.Speed);
                var (toX, toZ) = _stage.ConvertNoteCoordToWorldPosition(to.PositionCoord - new NoteCoord(0f, currentTime), to.Speed);
                _linkLine = (new Vector2(fromX, fromZ), new Vector2(toX, toZ));
            }
            else {
                _linkLine = null;
            }
        }

        private void SetHoldBodyDisplayLength()
        {
            float time;
            bool isHolding;
            if (_state is NoteDisplayState.Fall) {
                time = NoteModel.GetActualDuration();
                isHolding = false;
            }
            else if (_state is NoteDisplayState.Holding) {
                time = _stageDeltaTime + NoteModel.GetActualDuration();
                isHolding = true;
            }
            else {
                time = 0f;
                isHolding = false;
            }

            _stage.AssertStageLoaded();

            var scaleY = _stage.ConvertNoteCoordTimeToHoldScaleY(time, NoteModel.Speed);
            SetHoldScaleY(scaleY, isHolding);
        }

        protected abstract void SetHoldScaleY(float scaleY, bool isHolding);

        private void SetNoteSpriteColor()
        {
            SetNoteSpriteEditorStatus(NoteModel.IsSelected, NoteModel.IsCollided);
        }

        protected abstract void SetNoteSpriteEditorStatus(bool selected, bool collided);

        private void SetAppearAheadTime0SuddenPlus(GameStageNoteController? previousStageNote)
        {
            _stage.AssertStageLoaded();

            if (previousStageNote is null) {
                _appearAheadTime0SuddenPlus = _stage.GetStageNoteActiveAheadTime(NoteModel.Speed);
                return;
            }

            var prevNoteAppearAheadTime = previousStageNote._appearAheadTime0SuddenPlus;
            var prevNoteAppearTime = previousStageNote.NoteModel.Time - prevNoteAppearAheadTime;
            var noteAppearTime = _stage.GetStageNoteActiveTime(NoteModel);
            if (prevNoteAppearTime <= noteAppearTime) {
                _appearAheadTime0SuddenPlus = _stage.GetStageNoteActiveAheadTime(NoteModel.Speed);
                return;
            }

            _appearAheadTime0SuddenPlus = NoteModel.Time - prevNoteAppearTime;
        }

        private void SetState()
        {
            var state = GetState();
            if (Utils.SetField(ref _state, state)) {
                OnStateChanged(state);
            }
        }

        protected abstract void OnStateChanged(NoteDisplayState state);

        #endregion

        protected enum NoteDisplayState
        {
            Inactive,
            Invisible,
            Fall,
            Holding,
            HitEffect,
        }
    }
}