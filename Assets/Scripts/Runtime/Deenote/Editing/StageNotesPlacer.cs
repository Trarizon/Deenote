using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.CoreB;
using Deenote.CoreB.Notification;
using Deenote.EditorModels;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Editing
{
    internal sealed class StageNotesPlacer2 : INotifyPropertyChanged<StageNotesPlacer2>
    {
        #region Constants

        /// <summary>
        /// Press and drag mouse horizontal, the note will change to a swipe
        /// when drag horizontal delta value greater than this
        /// </summary>
        private const float SwipeHorizontalDragDeltaThreshold = 30f;
        /// <summary>
        /// If music time changed, after enter note-placing state, if music time delta is greater
        /// than the value, we never place a swipe note, and always place hold
        /// </summary>
        private const float SwipeVerticalDragDeltaTimeThreshold = 60f / 160f * 4;
        /// <summary>
        /// Press and drag mouse vertical, the note will change to a hold
        /// when drag vertical delta value is greater than this
        /// </summary>
        private const float HoldVerticalDragDeltaThreshold = 30f;
        /// <summary>
        /// If music time changed after enter note-placing state, if music time delta is greater
        /// than the value, we still placing a hold
        /// </summary>
        private const float HoldVerticalDragDeltaTimeThreshold = 60f / 160f / 4;
        /// <summary>
        /// If the cotangent of angle of drag direction and horizontal line is less than this
        /// The note is a swipe, otherwise, a hold
        /// </summary>
        private const float SwipeDragAngleCotangent = 1.7320508076f; // Sqrt(3)

        #endregion

        private GamePlayManager2 _gamePlayManager;

        private NotePrototypeModel _metaPrototype;
        private ObjectPool<NotePrototypeModel> _prototypePool;
        public List<NotePrototypeModel> Prototypes { get; } = new();

        public event Action<StageNotesPlacer2>? PrototypesChanged;
        public event Action<StageNotesPlacer2, PropertyChangedEventArgs>? PropertyChanged;

        // Properties

        private bool _snapToPositionGrids_bf;
        public bool SnapToPositionGrids
        {
            get => _snapToPositionGrids_bf;
            set {
                if (_snapToPositionGrids_bf != value) {
                    _snapToPositionGrids_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(SnapToPositionGrids)));
                }
            }
        }

        private bool _snapToTimeGrids_bf;
        public bool SnapToTimeGrids
        {
            get => _snapToTimeGrids_bf;
            set {
                if (_snapToTimeGrids_bf != value) {
                    _snapToTimeGrids_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(SnapToTimeGrids)));
                }
            }
        }

        // States

        private NoteCoord _anchorCoord_bf;
        public NoteCoord AnchorCoord
        {
            get => _anchorCoord_bf;
            set {
                if (_anchorCoord_bf != value) {
                    _anchorCoord_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(AnchorCoord)));
                }
            }
        }

        private bool _placeSoundedNote_bf;
        public bool PlaceSoundedNote
        {
            get => _placeSoundedNote_bf;
            set {
                if (_placeSoundedNote_bf != value) {
                    _placeSoundedNote_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(PlaceSoundedNote)));
                }
            }
        }

        private bool _placeSpeedWarningNote_bf;
        public bool PlaceSpeedWarningNote
        {
            get => _placeSpeedWarningNote_bf;
            set {
                if (_placeSpeedWarningNote_bf != value) {
                    _placeSpeedWarningNote_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(PlaceSpeedWarningNote)));
                }
            }
        }

        private bool _isPastingNote_bf;
        public bool IsPastingNoteRequested
        {
            get => _isPastingNote_bf;
            private set {
                if (_isPastingNote_bf != value) {
                    _isPastingNote_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(IsPastingNoteRequested)));
                }
            }
        }

        private bool _pastingRememberPosition_bf;
        public bool IsPastingRememberPosition
        {
            get => _pastingRememberPosition_bf;
            set {
                if (_pastingRememberPosition_bf != value) {
                    _pastingRememberPosition_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(IsPastingRememberPosition)));
                }
            }
        }

        private bool _isPlacingSlideRequested_bf;
        public bool IsPlacingSlideRequested
        {
            get => _isPlacingSlideRequested_bf;
            set {
                if (_isPlacingSlideRequested_bf != value) {
                    _isPlacingSlideRequested_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(IsPlacingSlideRequested)));
                }
            }
        }

        public StageNotesPlacer2(GamePlayManager2 gamePlayManager)
        {
            _gamePlayManager = gamePlayManager;

            _metaPrototype = new NotePrototypeModel();
            _prototypePool = new ObjectPool<NotePrototypeModel>(
                () => new NotePrototypeModel(),
                collectionCheck: false
            );

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Set("editor/snap_pos", SnapToPositionGrids);
                configs.Set("editor/snap_time", SnapToTimeGrids);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                SnapToPositionGrids = configs.GetBoolean("editor/snap_pos", true);
                SnapToTimeGrids = configs.GetBoolean("editor/snap_time", true);
            };
        }

        public void RequestPaste()
        {
            IsPastingNoteRequested = true;
        }

        public void PrepareSingle()
        {
            var prototype = _prototypePool.Get();
            _metaPrototype.CloneTo(prototype);
            Prototypes.Replace(MemoryMarshal.CreateReadOnlySpan(ref prototype, 1));
            PrototypesChanged?.Invoke(this);
        }

        public void PrepareSlides()
        {
            var prototype = _prototypePool.Get();
            _metaPrototype.CloneTo(prototype);
            _metaPrototype.Kind = NoteModel.NoteKind.Slide;
            Prototypes.Replace(MemoryMarshal.CreateReadOnlySpan(ref prototype, 1));
            PrototypesChanged?.Invoke(this);
        }

        public void PrepareTemplate(ReadOnlySpan<NotePrototypeModel> notes, NoteCoord baseCoord)
        {
            if (notes.IsEmpty)
                return;

            IsPastingNoteRequested = true;
            _templateBaseCoord = baseCoord;

            using var so = SpanOwner<NotePrototypeModel>.Allocate(notes.Length);
            var span = so.Span;
            for (int i = 0; i < notes.Length; i++) {
                span[i] = _prototypePool.Get();
                notes[i].CloneTo(span[i]);
            }
            NoteLinkHelpers.CloneLinkInfos(notes, span);
            Prototypes.Replace(span);
            PrototypesChanged?.Invoke(this);
        }

        private NoteCoord _templateBaseCoord;

        private NoteCoord _startCoord; // coord where mouse pressed
        private NoteCoord _startCoordQuantized; // coord where note placed, quantized
        private NoteCoord _dragPrevCoord; // coord on previous frame, not quantized

        public void BeginPlaceSingle(NoteCoord coord)
        {
            _startCoord = coord;
            _startCoordQuantized = MainSystem.GamePlayManager.Grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
            AnchorCoord = _startCoordQuantized;
        }

        public void BeginPlaceSlides(NoteCoord coord)
        {
            _startCoord = coord;
            _startCoordQuantized = MainSystem.GamePlayManager.Grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
            _dragPrevCoord = coord;
            AnchorCoord = _startCoordQuantized;
        }

        public void EndPlace()
        {
            IsPastingNoteRequested = false;

            if (Prototypes.Count == 1) {
                var prototype = Prototypes[0];
                var data = prototype.CreateAt(AnchorCoord);
                ClampNote(data);
                MainSystem.StageChartEditor.AddNote(data, data.PositionCoord);
                return;
            }

            var prototypes = Prototypes;
            using var so = SpanOwner<NoteModel>.Allocate(prototypes.Count);
            var span = so.Span;
            for (int i = 0; i < prototypes.Count; i++) {
                span[i] = prototypes[i].CreateAt(AnchorCoord);
                ClampNote(span[i]);
            }
            NoteLinkHelpers.CloneLinkInfos(prototypes.AsSpan(), span);
            MainSystem.StageChartEditor.AddMultipleNotes(span, span[0].PositionCoord);
        }

        public void MovingPlaceSingle(NoteCoord coord, Vector2 screenDelta)
        {
            // Draw swipe note when the angle of dragged is in 30 degree
            screenDelta.y *= SwipeDragAngleCotangent;

            // Drag horizontal
            if (screenDelta.x > SwipeHorizontalDragDeltaThreshold && screenDelta.x > screenDelta.y) {
                // When music time changed to much, place hold note
                if (Mathf.Abs(coord.Time - _startCoordQuantized.Time) > SwipeVerticalDragDeltaTimeThreshold) {
                    Hold(coord);
                    goto Last;
                }
                else {
                    Swipe();
                    goto Last;
                }
            }
            // Drag vertical
            if (screenDelta.y >= HoldVerticalDragDeltaThreshold) {
                Hold(coord);
                goto Last;
            }
            // No drag, but music changed to much, place hold
            if (Mathf.Abs(coord.Time - _startCoordQuantized.Time) >= HoldVerticalDragDeltaTimeThreshold) {
                Hold(coord);
                goto Last;
            }
            // No drag
            Click();

        Last:
            PrototypesChanged?.Invoke(this);
            return;

            void Swipe()
            {
                var prototype = Prototypes[0];
                prototype.Kind = NoteModel.NoteKind.Swipe;
                prototype.Duration = 0f;
                // If the note is previous a hold by dragging down, the position may be changed,
                // we should reset here
                prototype.Position = 0;
                prototype.Time = 0;
            }

            void Hold(NoteCoord coord)
            {
                var prototype = Prototypes[0];
                prototype.Kind = IsPlacingSlideRequested ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;

                var endTime = MainSystem.GamePlayManager.Grids.Quantize(coord, false, SnapToTimeGrids).Time;
                if (endTime >= _startCoordQuantized.Time) {
                    prototype.Duration = endTime - _startCoordQuantized.Time;
                    prototype.Position = 0;
                    prototype.Time = 0;
                }
                else {
                    var duration = _startCoordQuantized.Time - endTime;
                    prototype.Duration = duration;
                    prototype.Position = 0;
                    prototype.Time = -duration;
                }
            }

            void Click()
            {
                var prototype = Prototypes[0];
                prototype.Kind = IsPlacingSlideRequested ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                prototype.Duration = 0f;
                prototype.Position = 0;
                prototype.Time = 0;
            }
        }

        public void MovingPlaceSlides(NoteCoord coord)
        {
            var prevCoord = _dragPrevCoord;
            var startCoord = _startCoord;
            // When mouse drag over a time grid, generate an indicator at the position
            if (coord.Time >= startCoord.Time) {
                if (prevCoord.Time < startCoord.Time) {
                    // If mouse is blow the base note at prev frame, remove all extra notes
                    Prototypes.RemoveRange(..^1);
                    prevCoord = startCoord;
                }
                if (coord.Time > prevCoord.Time)
                    AtUpMoveUp(prevCoord, coord);
                else if (coord.Time < prevCoord.Time)
                    AtUpMoveDown(prevCoord, coord);
            }
            else {
                if (coord.Time < prevCoord.Time) {
                    Prototypes.RemoveRange(..^1);
                    prevCoord = startCoord;
                }
                if (coord.Time < prevCoord.Time)
                    AtDownMoveDown(prevCoord, coord);
                else if (coord.Time > prevCoord.Time)
                    AtDownMoveUp(prevCoord, coord);
            }

            PrototypesChanged?.Invoke(this);
            return;

            // TODO: arch1.1: I should complete these after grid refactored.
            void AtUpMoveUp(NoteCoord prevCoord, NoteCoord coord)
            {
                Debug.Assert(prevCoord.Time < coord.Time);
                Debug.Assert(Prototypes.All(x => x.Time <= prevCoord.Time));

                // Find all grids between prevCoord and coord, and interpolate notes between them
                var cmpTime = prevCoord.Time;
            }

            void AtUpMoveDown(NoteCoord prevCoord, NoteCoord coord)
            {

            }

            void AtDownMoveUp(NoteCoord prevCoord, NoteCoord coord)
            {

            }

            void AtDownMoveDown(NoteCoord prevCoord, NoteCoord coord)
            {

            }
        }

        public void MovingPlaceTemplate(NoteCoord coord)
        {
            if (IsPastingRememberPosition) {
                coord.Position = _templateBaseCoord.Position;
                coord = MainSystem.GamePlayManager.Grids.Quantize(coord, false, SnapToTimeGrids);
            }
            else {
                coord = MainSystem.GamePlayManager.Grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
            }
            AnchorCoord = coord;
        }

        public void MovingIdle(NoteCoord coord)
        {
            AnchorCoord = MainSystem.GamePlayManager.Grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
        }

        private void ClampNote(NoteModel note)
        {
            note.PositionCoord = NoteCoord.Clamp(note.PositionCoord, _gamePlayManager.GamePlayLength);
            note.Duration = Mathf.Min(note.Duration, _gamePlayManager.GamePlayLength - note.Time);
        }
    }
}