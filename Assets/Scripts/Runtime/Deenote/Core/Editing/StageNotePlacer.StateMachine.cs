#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.Core.Editing
{
    partial class StageNotePlacer
    {
        private StateFlag _currentState;
        private NoteCoord _updateNoteCoord;
        private Vector2 _updateMousePosition;

        public bool IsPlacing => _currentState is StateFlag.PlacingSingleNote or StateFlag.PlacingSlides or StateFlag.PlacingPastedNotes;

        public void BeginPlaceNote(NoteCoord coord, Vector2 mousePosition)
        {
            if (GetPlacementArea(coord) is PlacementArea.Invalid) {
                SetIndicatorsVisibility(PlacementArea.Invalid);
                return;
            }

            switch (_currentState) {
                case StateFlag.Idle:
                    _currentState = BeginPlaceSingleNote(coord, mousePosition);
                    break;
                case StateFlag.IdlePlacingSlides:
                    _currentState = BeginDragPlaceSlides(coord, mousePosition);
                    break;
                case StateFlag.IdlePastingNotes:
                    _currentState = BeginPasteNotes(coord, mousePosition);
                    break;
                case StateFlag.IdlePlacingSpeedChangeWarningNote:
                    _currentState = BeginPlaceSpeedChangeWarning(coord, mousePosition);
                    break;
                default:
                    return;
            }
            RefreshIndicatorVisibility();
        }

        public void UpdatePlaceNote(NoteCoord coord, Vector2 mousePosition, bool remainIndicatorVisibility = false)
        {
            _updateNoteCoord = coord;
            _updateMousePosition = mousePosition;

            switch (_currentState) {
                case StateFlag.Idle or StateFlag.IdlePlacingSlides or StateFlag.IdlePlacingSpeedChangeWarningNote:
                    _currentState = UpdateIdleMousePosition(coord, mousePosition);
                    break;
                case StateFlag.IdlePastingNotes or StateFlag.PlacingPastedNotes:
                    UpdatePasteNotes(coord, mousePosition);
                    break;
                case StateFlag.PlacingSingleNote:
                    UpdatePlaceSingleNote(coord, mousePosition);
                    break;
                case StateFlag.PlacingSlides:
                    UpdateDragPlaceSlides(coord, mousePosition);
                    break;
                case StateFlag.PlacingSpeedChangeWaringNote:
                    UpdatePlaceSpeedChangeWarning(coord, mousePosition);
                    break;
                default:
                    return;
            }
            if (!remainIndicatorVisibility) {
                RefreshIndicatorVisibility();
            }
        }

        public void EndPlaceNote(NoteCoord coord, Vector2 mousePosition)
        {
            switch (_currentState) {
                case StateFlag.PlacingSingleNote:
                    _currentState = EndPlaceSingleNote(coord, mousePosition);
                    break;
                case StateFlag.PlacingSlides:
                    _currentState = EndDragPlaceSlides(coord, mousePosition);
                    break;
                case StateFlag.PlacingPastedNotes:
                    _currentState = EndPasteNotes(coord, mousePosition);
                    ResetNotePrototypesToIdle();
                    break;
                case StateFlag.PlacingSpeedChangeWaringNote:
                    _currentState = EndPlaceSpeedChangeWarning(coord, mousePosition);
                    break;
                default:
                    return;
            }
            RefreshIndicatorVisibility();
        }

        public void CancelPlaceNote()
        {
            _currentState = GetIdlePlacingFlag();
            ResetNotePrototypesToIdle();
        }

        private void SwitchPlaceSlideModifier(bool flag)
        {
            switch (_currentState, flag) {
                case (StateFlag.Idle, true):
                    _currentState = StateFlag.IdlePlacingSlides;
                    SyncPrototypes();
                    break;
                case (StateFlag.IdlePlacingSlides, false):
                    _currentState = StateFlag.Idle;
                    SyncPrototypes();
                    break;
                case (StateFlag.PlacingSingleNote, _):
                    UpdatePlaceSingleNote(_updateNoteCoord, _updateMousePosition);
                    break;
            }

            void SyncPrototypes()
            {
                _prototypes[0].Kind = _metaPrototype.Kind;
                _indicators[0].Refresh();
            }
        }

        private void ResetNotePrototypesToIdle()
        {
            var note = _prototypes[0];
            _metaPrototype.CloneDataTo(note, true);
            note.UnlinkWithoutCutChain(keepNoteKind: true);
            _prototypes.SetCount(1);
            RefreshIndicators();
            SetPlacingNoteSpeed(null, false);
        }

        public void DisablePlaceNote()
        {
            SetIndicatorsVisibility(PlacementArea.Invalid);
        }

        internal void PreparePasteClipBoard()
        {
            if (_editor.ClipBoard.Notes.IsEmpty)
                return;

            if (_currentState is StateFlag.Idle or StateFlag.IdlePlacingSlides) {
                _currentState = StateFlag.IdlePastingNotes;
                using (var resetter = _prototypes.Resetting(_editor.ClipBoard.Notes.Length)) {
                    var baseCoord = _editor.ClipBoard.BaseCoord;
                    foreach (var cnote in _editor.ClipBoard.Notes) {
                        resetter.Add(out var note);
                        cnote.CloneDataTo(note, cloneSounds: true);
                    }
                }
                RefreshIndicators();
                SetPlacingNoteSpeed(_prototypes[0].Speed, false);
            }
        }

        private partial StateFlag UpdateIdleMousePosition(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginPlaceSingleNote(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdatePlaceSingleNote(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndPlaceSingleNote(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginDragPlaceSlides(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdateDragPlaceSlides(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndDragPlaceSlides(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginPasteNotes(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdatePasteNotes(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndPasteNotes(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginPlaceSpeedChangeWarning(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdatePlaceSpeedChangeWarning(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndPlaceSpeedChangeWarning(NoteCoord coord, Vector2 mousePosition);

        /// <summary>
        /// Is in state that force show indicator
        /// </summary>
        /// <returns></returns>
        private bool IsForceShowIndicator()
        {
            return _currentState is not (StateFlag.Disabled or StateFlag.Idle);
        }

        private bool IsFreezeNotePrototypeProperties()
        {
            return _currentState is StateFlag.IdlePastingNotes or StateFlag.PlacingPastedNotes;
        }

        private StateFlag GetIdlePlacingFlag()
        {
            switch (GetPlacementArea(_updateNoteCoord)) {
                case PlacementArea.NotePlacement:
                    if (PlaceSlideModifier)
                        return StateFlag.IdlePlacingSlides;
                    else
                        return StateFlag.Idle;
                case PlacementArea.SpeedChangeWarning:
                    return StateFlag.IdlePlacingSpeedChangeWarningNote;
                default:
                    return StateFlag.Idle;
            }
        }

        /// <summary>
        /// Get if idle, which state will this instance be
        /// </summary>
        /// <returns>
        /// <see cref="StateFlag.Idle"/> or <see cref="StateFlag.IdlePlacingSlides"/>
        /// </returns>
        private StateFlag GetIdlePlacingNoteFlag()
        {
            if (PlaceSlideModifier)
                return StateFlag.IdlePlacingSlides;
            else
                return StateFlag.Idle;
        }

        private PlacementArea GetPlacementArea(StateFlag flag)
        {
            switch (flag) {
                case StateFlag.Idle:
                case StateFlag.IdlePlacingSlides:
                case StateFlag.IdlePastingNotes:
                case StateFlag.PlacingSingleNote:
                case StateFlag.PlacingSlides:
                case StateFlag.PlacingPastedNotes:
                    return PlacementArea.NotePlacement;
                case StateFlag.IdlePlacingSpeedChangeWarningNote:
                case StateFlag.PlacingSpeedChangeWaringNote:
                    return PlacementArea.SpeedChangeWarning;
                default:
                    return PlacementArea.Invalid;
            }
        }

        public enum StateFlag
        {
            /// <summary>
            /// When disabled, the indicator is forced to be hidden, and all placement will be disabled
            /// </summary>
            Disabled,
            Idle,
            IdlePlacingSlides,
            IdlePastingNotes,
            IdlePlacingSpeedChangeWarningNote,
            PlacingSingleNote,
            PlacingSlides,
            PlacingPastedNotes,
            PlacingSpeedChangeWaringNote,
        }
    }
}