#nullable enable

using Deenote.Core.GamePlay;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library;
using System;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal abstract class GameStageNoteController : MonoBehaviour
    {
        protected GamePlayManager _game = default!;
        protected GameStageNotePlaneController _plane = default!;

        public NoteEditorModel NoteModel { get; private set; } = default!;

        private (Vector2, Vector2)? _linkLine;
        private float _noteColorAlpha;

        [SerializeField]
        private NotePlayState _playState;

        // Appear ahead time of note when sudden+ is 0,
        // The value may be affected if the note is following a high-speed note
        private float _appearAheadTime0SuddenPlus;
        protected float _stageDeltaTime;

        // The actual appear ahead time, the value 
        protected float AppearAheadTime
        {
            get {
                _game.AssertStageLoaded();

                var suddenPlusAheadTime = _game.Stage.EvaluateNoteAppearAheadTime(NoteModel.Speed);
                float aheadTime;
                if (_game.EarlyDisplaySlowNotes) {
                    aheadTime = suddenPlusAheadTime;
                }
                else {
                    aheadTime = Mathf.Min(suddenPlusAheadTime, _appearAheadTime0SuddenPlus);
                }
                return aheadTime;
            }
        }

        internal void OnInstantiate(GameStageNotePlaneController plane)
        {
            _plane = plane;
            _game = _plane.GameStage.GamePlay;
            _plane.GameStage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal void Initialize(NoteEditorModel noteModel)
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
            _plane.GameStage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _playState = NotePlayState.Inactive;
            SetLinkLine();
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (_linkLine is var (start, end)) {
                _game.AssertStageLoaded();
                collector.AddLine(start, end,
                    _game.Stage.GridLineArgs.LinkLineColor with { a = _noteColorAlpha },
                    _game.Stage.GridLineArgs.LinkLineWidth);
            }
        }

        #region Refresh

        /// <summary>
        /// Called when music time updated
        /// </summary>
        private void RefreshTimeDisplayState()
        {
            //SetState();
            switch (_playState) {
                case NotePlayState.Invisible:
                    SetLinkLine();
                    break;
                case NotePlayState.Fall:
                    SetNotePositionZ(_stageDeltaTime);
                    //SetNoteSpriteAlpha();
                    SetLinkLine();
                    SetHoldingStatus();
                    //SetHoldBodyDisplayLength();
                    break;
                case NotePlayState.Holding:
                    SetNotePositionZ(0f);
                    SetLinkLine();
                    SetHoldingStatus();
                    //SetHoldBodyDisplayLength();
                    //SetHoldingHitEffect();
                    break;
                case NotePlayState.HitEffect:
                    SetNotePositionZ(0f);
                    SetLinkLine();
                    SetHitEffect();
                    break;
            }
        }

        private NotePlayState GetState()
        {
            if (IsInvisible())
                return NotePlayState.Invisible;
            if (_stageDeltaTime >= 0)
                return NotePlayState.Fall;
            if (_stageDeltaTime > -NoteModel.GetActualDuration())
                return NotePlayState.Holding;
            return NotePlayState.HitEffect;

            bool IsInvisible()
            {
                if (_stageDeltaTime >= AppearAheadTime)
                    return true;

                if (!_game.EarlyDisplaySlowNotes) {
                    // In TimeOrder mode, the note should display only after its previous note displayed
                    if (_game.NotesManager.GetNextActiveNodeInTimeOrderDisplayMode() is { } next) {
                        if (ModelComparers.ViaTimeUnique.Compare(NoteModel, next) >= 0) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void RefreshStageDeltaTime()
        {
            SetNoteRelativeTime();
            //_stageDeltaTime = NoteModel.Time - _game.MusicPlayer.Time;
            //RefreshTimeDisplayState();
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
            _game.AssertStageLoaded();

            SetNotePositionX();
            //SetNoteSprite();
            SetNoteHeadKind();
            SetIsHold();
            SetHoldingStatus();
            SetNoteSize();
            SetHighlightState();
            SetLinkLine();
        }

        //public void RefreshColorAlpha()
        //{
        //    if (_playState is NotePlayState.Invisible or NotePlayState.Fall) {
        //        SetState();
        //        if (_playState is NotePlayState.Invisible)
        //            SetLinkLine();
        //        if (_playState is NotePlayState.Fall)
        //            SetNoteSpriteAlpha();
        //    }
        //}

        public void RefreshHighlightState()
        {
            if (_playState is not NotePlayState.Fall)
                return;

            SetHighlightState();
        }


        #endregion

        private NoteHeadKind _noteHeadKind;

        #region New Setters

        private void SetNoteRelativeTime()
        {
            _stageDeltaTime = NoteModel.Time - _game.MusicPlayer.Time;
            if (_stageDeltaTime > AppearAheadTime) {
                goto Invisible;
            }
            if (!_game.EarlyDisplaySlowNotes &&
                // In TimeOrder mode, the note should display only after its previous note displayed
                _game.NotesManager.GetNextActiveNodeInTimeOrderDisplayMode() is { } next &&
                ModelComparers.ViaTimeUnique.Compare(NoteModel, next) >= 0) {
                goto Invisible;
            }
            if (_stageDeltaTime >= 0) {
                goto Fall;
            }
            if (_stageDeltaTime > -NoteModel.GetActualDuration()) {
                goto Holding;
            }
            goto HitEffect;

        Invisible:
            if (Utils.SetField(ref _playState, NotePlayState.Invisible)) {
                OnPlayStateChanged(_playState);
            }
            SetLinkLine();
            return;

        Fall:
            if (Utils.SetField(ref _playState, NotePlayState.Fall)) {
                OnPlayStateChanged(_playState);
            }
            SetNotePositionZ(_stageDeltaTime);
            OnFallingProgressChanged(_stageDeltaTime, AppearAheadTime);
            SetLinkLine();
            SetHoldingStatus();
            return;

        Holding:
            if (Utils.SetField(ref _playState, NotePlayState.Holding)) {
                OnPlayStateChanged(_playState);
            }
            SetNotePositionZ(0f);
            SetLinkLine();
            SetHoldingStatus();
            return;

        HitEffect:
            if (Utils.SetField(ref _playState, NotePlayState.HitEffect)) {
                OnPlayStateChanged(_playState);
            }
            SetNotePositionZ(0);
            SetLinkLine();
            SetHitEffect();
            return;
        }

        private void SetNoteHeadKind()
        {
            var value = NoteModel switch {
                { Kind: NoteKind.Swipe } => NoteHeadKind.Swipe,
                { Kind: NoteKind.Slide } => NoteHeadKind.Slide,
                { HasSounds: true } => NoteHeadKind.Click,
                _ => NoteHeadKind.NoSound,
            };

            if (Utils.SetField(ref _noteHeadKind, value)) {
                OnNoteHeadKindChanged(value);
            }
        }

        private void SetIsHold()
        {
            OnNoteIsHoldChanged(NoteModel.IsHold);
        }

        private void SetHoldingStatus()
        {
            var relativeTime = _game.MusicPlayer.Time - NoteModel.Time;
            OnHoldStatusChanged(new NoteHoldingStatus(relativeTime, NoteModel.Duration));
        }

        private void SetHighlightState()
        {
            var flags = NoteHighlightFlags.None;
            if (NoteModel.IsSelected)
                flags |= NoteHighlightFlags.Selected;
            if (NoteModel.IsCollided)
                flags |= NoteHighlightFlags.Collided;
            if (!_game.IsNoteHighlighted(NoteModel))
                flags |= NoteHighlightFlags.Downplayed;
            OnNoteHighlightChanged(flags);
        }

        private void SetHitEffect()
        {
            var time = _game.MusicPlayer.Time - NoteModel.EndTime;
            OnHitEffectTimeChanged(time);
        }

        private void SetNoteSize()
        {
            OnNoteSizeChanged(NoteModel.Size);
        }

        #endregion

        // |            *      |
        // ^judge line  ^note  ^note appear
        // |<timeToHit->|
        // |<-appearAheadTime->|
        protected abstract void OnFallingProgressChanged(float timeToHit, float appearAheadTime);
        protected abstract void OnNoteHighlightChanged(NoteHighlightFlags flags);
        protected abstract void OnNoteHeadKindChanged(NoteHeadKind kind);
        protected abstract void OnNoteIsHoldChanged(bool isHold);
        protected abstract void OnHoldStatusChanged(NoteHoldingStatus status);
        protected abstract void OnHitEffectTimeChanged(float time);
        protected abstract void OnNoteSizeChanged(float size);

        #region Setters

        private void SetNotePositionX()
        {
            transform.WithLocalPositionX(_game.Stage!.ConvertNotePositionToWorldX(NoteModel.Position));
        }

        private void SetNotePositionZ(float time)
        {
            _game.AssertStageLoaded();

            float z = _game.Stage.EvaluateNoteWorldZ(time, NoteModel.Speed);
            transform.WithLocalPositionZ(z);
        }

        //protected void SetNoteSpriteAlpha()
        //{
        //    _game.AssertStageLoaded();
        //    Debug.Assert(_playState is NotePlayState.Fall);

        //    var appearAheadTime = AppearAheadTime;
        //    var noteFadeInEndTime = appearAheadTime * (1 - _game.Stage.Args.NoteFadeInRangePercent);

        //    var maxAlpha = _game.IsFilterNoteSpeed && !Mathf.Approximately(NoteModel.Speed, _game.HighlightedNoteSpeed)
        //        ? _game.Stage.Args.NoteDownplayAlpha
        //        : 1f;
        //    _noteColorAlpha = MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0, maxAlpha);

        //    SetNoteSpriteRendererAlpha(_noteColorAlpha);
        //}

        //protected abstract void SetNoteSpriteRendererAlpha(float alpha);

        private void SetLinkLine()
        {
            _game.AssertStageLoaded();

            if (_playState is NotePlayState.Fall && _game.IsShowLinkLines && NoteModel.NextLink is not null) {
                var currentTime = _game.MusicPlayer.Time;

                var to = NoteModel.NextLink;
                var from = NoteModel;

                var (fromX, fromZ) = _game.Stage.EvaluateNoteWorldXZ(from.PositionCoord - new NoteCoord(0f, currentTime), from.Speed);
                var (toX, toZ) = _game.Stage.EvaluateNoteWorldXZ(to.PositionCoord - new NoteCoord(0f, currentTime), to.Speed);
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
            if (_playState is NotePlayState.Fall) {
                time = NoteModel.GetActualDuration();
                isHolding = false;
            }
            else if (_playState is NotePlayState.Holding) {
                time = _stageDeltaTime + NoteModel.GetActualDuration();
                isHolding = true;
            }
            else {
                time = 0f;
                isHolding = false;
            }

            _game.AssertStageLoaded();

            //var scaleY = _game.ConvertNoteCoordTimeToHoldScaleY(time, NoteModel.Speed);
            //SetHoldScaleY(scaleY, isHolding);
        }

        //protected abstract void SetHoldScaleY(float scaleY, bool isHolding);

        //private void SetNoteSpriteColor()
        //{
        //    _game.AssertStageLoaded();
        //    var stage = _game.Stage;

        //    Color color;
        //    if (NoteModel.IsSelected)
        //        color = stage.Args.NoteSelectedColor;
        //    else if (NoteModel.IsCollided)
        //        color = stage.Args.NoteCollidedColor;
        //    else
        //        color = Color.white;
        //    //SetNoteSpriteColorRGB(color);
        //}

        //protected abstract void SetNoteSpriteColorRGB(Color color);

        private void SetAppearAheadTime0SuddenPlus(GameStageNoteController? previousStageNote)
        {
            _game.AssertStageLoaded();

            if (previousStageNote is null) {
                _appearAheadTime0SuddenPlus = _game.Stage.EvaluateNoteActiveAheadTime(NoteModel.Speed);
                return;
            }

            var prevNoteAppearAheadTime = previousStageNote._appearAheadTime0SuddenPlus;
            var prevNoteAppearTime = previousStageNote.NoteModel.Time - prevNoteAppearAheadTime;
            var noteAppearTime = _game.Stage.EvaluateNoteActiveTime(NoteModel);
            if (prevNoteAppearTime <= noteAppearTime) {
                _appearAheadTime0SuddenPlus = _game.Stage.EvaluateNoteActiveAheadTime(NoteModel.Speed);
                return;
            }

            _appearAheadTime0SuddenPlus = NoteModel.Time - prevNoteAppearTime;
        }

        private void SetState()
        {
            var state = GetState();
            if (Utils.SetField(ref _playState, state)) {
                OnPlayStateChanged(state);
            }
        }

        protected abstract void OnPlayStateChanged(NotePlayState state);

        #endregion

        protected enum NotePlayState
        {
            Inactive,
            Invisible,
            Fall,
            Holding,
            HitEffect,
        }

        public enum NoteHeadKind
        {
            Invalid,
            Click,
            NoSound,
            Slide,
            Swipe,
        }

        [Flags]
        public enum NoteHighlightFlags
        {
            None = 0,
            Selected = 1 << 0,
            Collided = 1 << 1,
            Downplayed = 1 << 2,
        }

        public readonly struct NoteHoldingStatus
        {
            private readonly float _duration;
            private readonly float _passedTime;

            public bool IsHolding => _passedTime > 0f && _passedTime < _duration;
            public float Duration => _duration;
            public float PassedTime => _passedTime;
            public float RestTime => _passedTime < 0 ? _duration : _duration - _passedTime;

            internal NoteHoldingStatus(float passedTime, float duration)
            {
                _passedTime = passedTime;
                _duration = duration;
            }
        }
    }
}