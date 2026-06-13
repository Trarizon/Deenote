#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Core;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Editing.Grids;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Library.Collections;
using Deenote.Library.Mathematics;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Editing.NotePlacement
{
    public enum NotePlacingMode
    {
        Normal,
        SoundNote,
    }

    public sealed partial class StageNotePlacer2 : INotifyPropertyChanged<StageNotePlacer2>
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

        private readonly GamePlayContext _gamePlay;
        private readonly GridsContext _grids;
        private readonly NotePlacementContext _context;
        private readonly ChartNotesEditor _editor;
        private readonly InputInterpreter _inputInterpreter;

        private readonly NotePrototypeModel _metaPrototype;

        private bool _snapToPositionGrids_bf;
        private bool _snapToTimeGrids_bf;

        private bool? _indicatorsForceVisible_bf;
        private float? _forceDisplayPlacementNoteSpeed_bf;
        private bool _isPastingRequested_bf;
        private bool _isPlacingSlidesRequested_bf;
        private bool _isPastingRemeberPosition_bf;
        private bool _placeSoundNoteByDefault_bf;

        private bool _isIdle;
        private NoteCoord _startCoord;
        private NoteCoord _startCoordQuantized;

        public event Action<StageNotePlacer2, PropertyEventArgs>? PropertyChanged;

        internal StageNotePlacer2(GamePlayContext gamePlay, EditorContext editorContext, ChartNotesEditor editor, SaveSystem storage, InputInterpreter inputInterpreter)
        {
            _gamePlay = gamePlay;
            _grids = editorContext.Grids;
            _context = editorContext.NotePlacement;
            _editor = editor;
            _inputInterpreter = inputInterpreter;

            _metaPrototype = new NotePrototypeModel {
                Position = 0,
                Time = 0,
                Size = 1,
                Speed = 1
            };

            storage.SavingConfigurations += configs =>
            {
                configs.Set("editor/snap_pos", SnapToPositionGrids);
                configs.Set("editor/snap_time", SnapToTimeGrids);
            };

            storage.LoadedConfigurations += configs =>
            {
                SnapToPositionGrids = configs.GetBoolean("editor/snap_pos", true);
                SnapToTimeGrids = configs.GetBoolean("editor/snap_time", true);
            };
        }

        public void OnStart()
        {
            _context.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.PlacementNoteSpeed))) {
                    _metaPrototype.Speed = s.PlacementNoteSpeed;
                    NotifyMetaPrototypeChanged();
                }
            });

            var actions = _inputInterpreter.InputActions.EditorSettings;
            actions.SnapToGrids.started += _ =>
            {
                var val = !(SnapToPositionGrids && SnapToTimeGrids);
                SnapToPositionGrids = val;
                SnapToTimeGrids = val;
            };
            actions.PasteRememberPosition.started += _ => IsPastingRemeberPosition = true; ;
            actions.PasteRememberPosition.canceled += _ => IsPastingRemeberPosition = false;
            actions.PlaceNoteSlideFlag.started += _ => IsPlacingSlidesRequested = true;
            actions.PlaceNoteSlideFlag.canceled += _ => IsPlacingSlidesRequested = false;
            actions.PlaceSoundNote.started += _ => PlaceSoundNoteByDefault = true;
        }

        public void PrepareSingle()
        {
            _isIdle = true;
            var prototype = _metaPrototype.Clone();
            _context.ReplacePrototypes(MemoryMarshal.CreateReadOnlySpan(ref prototype, 1));
        }

        public void PrepareSlide()
        {
            _isIdle = true;
            var prototype = _metaPrototype.Clone();
            prototype.Kind = NoteKind.Slide;
            _context.ReplacePrototypes(MemoryMarshal.CreateReadOnlySpan(ref prototype, 1));
        }

        public void BeginPlaceSingleNote(NoteCoord coord)
        {
            _isIdle = false;
            _startCoord = coord;
            _startCoordQuantized = _grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
        }

        public void BeginPlaceSlides(NoteCoord coord)
        {
            _isIdle = false;
            _startCoord = coord;
            _startCoordQuantized = _grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
            _dragPrevCoord = coord;
        }

        public void EndPlace()
        {
            IsPastingRequested = false;

            if (_context.CurrentPrototypes.Count == 1) {
                var prototype = _context.CurrentPrototypes[0];
                var data = prototype.ToDataNonLinkInfo();
                data.PositionCoord += _startCoordQuantized;
                _editor.AddNote(data);
            }
            else {
                var prototypes = _context.CurrentPrototypes;
                using var so_notes = SpanOwner<NoteData>.Allocate(prototypes.Count);
                var span = so_notes.Span;
                for (int i = 0; i < prototypes.Count; i++) {
                    span[i] = prototypes[i].ToDataNonLinkInfo();
                    span[i].PositionCoord += _startCoordQuantized;
                }
                NoteLinkHelpers.CloneLinkInfos<NotePrototypeModel>(prototypes.AsSpan(), span);
                _editor.AddNotes(span);
            }
        }

        public void MovingPlaceSingle(NoteCoord coord, Vector2 delta, float timeDelta)
        {
            // Draw swipe note when the angle of dragged is in 30 degree
            delta.y *= SwipeDragAngleCotangent;

            // Drag horizontal
            if (delta.x > SwipeHorizontalDragDeltaThreshold && delta.x > delta.y) {
                // When music time changed to much, place hold note
                if (Mathf.Abs(timeDelta) > SwipeVerticalDragDeltaTimeThreshold) {
                    goto PlaceHold;
                }
                else {
                    goto PlaceSwipe;
                }
            }
            // Drag vertical
            if (delta.y >= HoldVerticalDragDeltaThreshold) {
                goto PlaceHold;
            }
            // No drag, but music changed to much, place hold note
            if (Mathf.Abs(timeDelta) >= HoldVerticalDragDeltaTimeThreshold) {
                goto PlaceHold;
            }
            // No drag
            else {
                goto PlaceClick;
            }


        PlaceSwipe:
            {
                var prototype = _context.CurrentPrototypes[0];
                prototype.Kind = NoteKind.Swipe;
                prototype.Duration = 0f;
                // If the note is previous a hold by dragging down, the position may be changed,
                // we should reset here
                prototype.PositionCoord = new(0, 0);
                return;
            }

        PlaceHold:
            {
                var prototype = _context.CurrentPrototypes[0];
                prototype.Kind = IsPlacingSlidesRequested ? NoteKind.Slide : NoteKind.Click;
                var drageEndCoordTime = _grids.Quantize(coord, false, SnapToTimeGrids).Time;
                drageEndCoordTime = Mathf.Min(drageEndCoordTime, _gamePlay.MusicLength);

                if (drageEndCoordTime >= _startCoordQuantized.Time) {
                    prototype.Duration = drageEndCoordTime - _startCoordQuantized.Time;
                    prototype.PositionCoord = new(0, 0);
                }
                else {
                    var duration = _startCoordQuantized.Time - drageEndCoordTime;
                    prototype.Duration = duration;
                    prototype.PositionCoord = new(0, -duration);
                }
                return;
            }

        PlaceClick:
            {
                var prototype = _context.CurrentPrototypes[0];
                prototype.Kind = IsPlacingSlidesRequested ? NoteKind.Slide : NoteKind.Click;
                prototype.Duration = 0;
                prototype.PositionCoord = new(0, 0);
            }
        }

        private NoteCoord _dragPrevCoord;
        public void MovingPlaceSlides(NoteCoord coord)
        {
            var prevCoord = _dragPrevCoord;
            var startCoord = _startCoord;
            // When mouse drag over a time grid, generate an indicator at the position
            if (coord.Time >= startCoord.Time) {
                if (prevCoord.Time < startCoord.Time) {
                    // If mouse is blow the base note at prev frame, remove all extra notes
                    _context.CurrentPrototypes.RemoveRange(..^1);
                    prevCoord = startCoord;
                }
                if (coord.Time > prevCoord.Time)
                    AtUpMoveUp(prevCoord, coord);
                else if (coord.Time < prevCoord.Time)
                    AtUpMoveDown(prevCoord, coord);
            }
            else {
                if (coord.Time < prevCoord.Time) {
                    _context.CurrentPrototypes.RemoveRange(..^1);
                    prevCoord = startCoord;
                }
                if (coord.Time < prevCoord.Time)
                    AtDownMoveDown(prevCoord, coord);
                else if (coord.Time > prevCoord.Time)
                    AtDownMoveUp(prevCoord, coord);
            }

            _dragPrevCoord = coord;

            void AtUpMoveUp(NoteCoord prevCoord, NoteCoord coord)
            {
                Debug.Assert(prevCoord.Time < coord.Time);
                Debug.Assert(_context.CurrentPrototypes.All(x => x.Time <= prevCoord.Time));

                // Find all grids between prevCoord and coord, and interpolate notes between them
                var cmpTime = prevCoord.Time;
                while (true) {
                    var nGrid = _grids.TimeGrids.CeilToNearestNextGrid(cmpTime);
                    if (!nGrid.HasValue) {
                        // No more grids
                        return;
                    }
                    if (nGrid.Time > coord.Time) {
                        return;
                    }

                    _context.AddPrototype(out var prototype);
                    NoteLinkHelpers.InsertAfter(prototype, _context.CurrentPrototypes[^2]);
                    InitPrototype(prototype, nGrid.Time, prevCoord, coord);
                    cmpTime = nGrid.Time;
                }
            }

            void AtUpMoveDown(NoteCoord prevCoord, NoteCoord coord)
            {
                Debug.Assert(prevCoord.Time > coord.Time);
                Debug.Assert(_context.CurrentPrototypes.All(x => x.Time <= prevCoord.Time));

                for (int i = _context.CurrentPrototypes.Count - 1; i >= 0; i--) {
                    var prototype = _context.CurrentPrototypes[i];
                    if (prototype.Time <= coord.Time) {
                        _context.RemovePrototypes((i + 1)..);
                        break;
                    }
                }
            }

            void AtDownMoveDown(NoteCoord prevCoord, NoteCoord coord)
            {
                Debug.Assert(prevCoord.Time > coord.Time);
                Debug.Assert(_context.CurrentPrototypes.All(x => x.Time >= prevCoord.Time));

                // Find all grids between prevCoord and coord, and interpolate notes between them
                var cmpTime = prevCoord.Time;
                while (true) {
                    var nGrid = _grids.TimeGrids.FloorToNearestNextGrid(cmpTime);
                    if (!nGrid.HasValue) {
                        // No more grids 
                        return;
                    }
                    if (nGrid.Time < coord.Time) {
                        return;
                    }

                    _context.InsertPrototype(0, out var prototype);
                    NoteLinkHelpers.InsertBefore(prototype, _context.CurrentPrototypes[1]);
                    InitPrototype(prototype, nGrid.Time, prevCoord, coord);
                    cmpTime = nGrid.Time;
                }
            }

            void AtDownMoveUp(NoteCoord prevCoord, NoteCoord coord)
            {
                Debug.Assert(prevCoord.Time < coord.Time);
                Debug.Assert(_context.CurrentPrototypes.All(x => x.Time >= prevCoord.Time));
                for (int i = 0; i < _context.CurrentPrototypes.Count; i++) {
                    var prototype = _context.CurrentPrototypes[i];
                    if (prototype.Time >= coord.Time) {
                        _context.RemovePrototypes(..i);
                        break;
                    }
                }
            }

            void InitPrototype(NotePrototypeModel prototype, float gridTime, NoteCoord prev, NoteCoord coord)
            {
                prototype.Kind = NoteKind.Slide;
                var actualCoord = new NoteCoord(
                    MathUtils.MapTo(gridTime, prev.Time, coord.Time, prev.Position, coord.Position),
                    gridTime);
                prototype.PositionCoord = coord - _startCoord;
            }
        }

        public void MovingPlaceTemplate(NoteCoord coord)
        {
            if (IsPastingRemeberPosition) {
                coord.Position = _templateBaseCoord.Position;
                coord = _grids.Quantize(coord, false, SnapToTimeGrids);
            }
            else {
                coord = _grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
            }
            _context.AnchorCoord = coord;
        }

        public void MovingIdle(NoteCoord coord)
        {
            coord = _grids.Quantize(coord, SnapToPositionGrids, SnapToTimeGrids);
            _context.AnchorCoord = coord;
        }

        private NoteCoord _templateBaseCoord;

        public void PrepareTemplateNotes(ReadOnlySpan<NoteData> notes)
        {
            if (notes.IsEmpty)
                return;

            _isIdle = true;
            IsPastingRequested = true;

            _templateBaseCoord = notes[0].PositionCoord;

            using var so = SpanOwner<NotePrototypeModel>.Allocate(notes.Length);
            var span = so.Span;
            for (int i = 0; i < notes.Length; i++) {
                var note = new NotePrototypeModel();
                note.FromDataNonLinkInfo(notes[i], _templateBaseCoord);
                span[i] = note;
            }
            NoteLinkHelpers.CloneLinkInfos(notes, (ReadOnlySpan<NotePrototypeModel>)span);
        }

        private void NotifyMetaPrototypeChanged()
        {
            if (_isIdle && !IsPastingRequested) {
                var prototype = _metaPrototype.Clone();
                _context.ReplacePrototypes(MemoryMarshal.CreateReadOnlySpan(ref prototype, 1));
            }
        }
    }
}
