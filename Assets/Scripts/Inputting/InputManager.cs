#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Contexts;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.Editing.Grids;
using Deenote.Editing.NoteSelection;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.InputSystem.InputActions;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.UI;
using Deenote.UIFramework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deenote.Inputting
{
    public sealed class InputManager : MonoBehaviour
    {
        private KeyboardShortcutInputActions _inputActions = default!;

        //private GamePlayManager _game = default!;
        private StageChartEditor _ceditor = default!;

        private GamePlayContext _gamePlayContext;
        private GamePlayManagerB _gamePlay;
        private ChartNotesEditor _editor = default!;
        private NoteSelectionContext _selection;
        private GridsContext _grids;
        private GameStageContext _stageContext;
        private ProjectContext _projectContext;


        private float? _musicResetTime;

        private bool _isEnabled;
        private bool _isGamePlayEnabled;

        private void Awake()
        {
            //_game = MainSystem.GamePlayManager;
            _ceditor = MainSystem.StageChartEditor;

            _gamePlayContext = MainSystem.Contexts.GamePlay;
            _gamePlay = MainSystem.GamePlayManagerB;
            _editor = MainSystem.ChartEditor;
            _selection = MainSystem.Contexts.Editor.NoteSelection;
            _grids = MainSystem.Contexts.Editor.Grids;
            _stageContext = MainSystem.Contexts.GameStage;
            _projectContext = MainSystem.Contexts.Project;

            _inputActions = new();

            RegisterStageGamePlay();
            RegisterStageSettings();
            RegisterNoteEdit();
            RegisterEditorSettings();
            RegisterProjectManagement();

            UISystem.FocusedControlChanged += ctrl =>
            {
                SetGeneralsEnable(ctrl is null);
            };

        }

        private void OnEnable()
        {
            SetGeneralsEnable(true);
            _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    SetGamePlayEnabled(s.CurrentChart is not null);
                }
            });
        }

        private void OnDisable()
        {
            SetGeneralsEnable(false);
        }

        private void Update()
        {
            Update_MouseAction();
        }


        private void SetGeneralsEnable(bool enable)
        {
            if (Utils.SetField(ref _isEnabled, enable)) {
                NotifyActionsEnable();
            }
        }

        private void SetGamePlayEnabled(bool enable)
        {
            if (Utils.SetField(ref _isGamePlayEnabled, enable)) {
                NotifyActionsEnable();
            }
        }

        private void NotifyActionsEnable()
        {
            if (_isEnabled) {
                _inputActions.StageSettings.Enable();
                _inputActions.EditorSettings.Enable();
                _inputActions.ProjectManagement.Enable();
                if (_isGamePlayEnabled) {
                    _inputActions.StageGamePlay.Enable();
                    _inputActions.NoteEdit.Enable();
                }
            }
            else {
                _inputActions.Disable();
            }
        }

        #region GamePlay

        private bool _manualPlaySpeedUp;
        private float _manualPlay;

        private void RegisterStageGamePlay()
        {
            var actions = _inputActions.StageGamePlay;
            actions.PauseResume.started += _ => _gamePlay.TogglePlayingState();
            actions.AutoResetPlay.started += _ =>
            {
                _musicResetTime = _gamePlayContext.CurrentTime;
                _gamePlay.Play();
            };
            actions.AutoResetPlay.canceled += _ =>
            {
                if (_musicResetTime is { } mrt) {
                    _gamePlay.Stop();
                    _gamePlayContext.CurrentTime = mrt;
                    _musicResetTime = null;
                }
            };
            actions.ToMusicStart.started += _ => _gamePlayContext.CurrentTime = 0f;
            actions.ToMusicEnd.started += _ => _gamePlayContext.CurrentTime = _gamePlayContext.MusicLength;
            actions.ManualPlay.started += context =>
            {
                _manualPlay = context.ReadValue<float>();
                SetManualPlay();
            };
            actions.ManualPlay.canceled += context =>
            {
                _manualPlay = 0f;
                _gamePlay.SetManualPlaySpeed(null);
            };
            actions.ManualPlaySpeedUp.started += _ =>
            {
                _manualPlaySpeedUp = true;
                if (_manualPlay != 0f) SetManualPlay();
            };
            actions.ManualPlaySpeedUp.canceled += _ =>
            {
                _manualPlaySpeedUp = false;
                if (_manualPlay != 0f) SetManualPlay();
            };
            actions.ScrollPlay.performed += context =>
            {
                if (!MainWindow.Views.PerspectiveViewPanelView.IsMouseHovering)
                    return;

                var delta = context.ReadValue<Vector2>().y;
                if (delta != 0f) {
                    var deltaTime = delta * 0.001f * MainSystem.GlobalSettings.GameViewScrollSensitivity;
                    _gamePlay.Nudge(-deltaTime);
                }
            };

            void SetManualPlay()
            {
                if (_manualPlaySpeedUp)
                    _gamePlay.SetManualPlaySpeed(5.0f * _manualPlay);
                else
                    _gamePlay.SetManualPlaySpeed(2.5f * _manualPlay);
            }
        }

        #endregion

        #region StageSettings

        private const int NoteFallSpeedDelta = 5;
        private const int MusicSpeedDelta = 1;

        private void RegisterStageSettings()
        {
            var actions = _inputActions.StageSettings;
            actions.EscapeFullScreen.started += _ => { MainWindow.Views.PerspectiveViewPanelView.SetIsFullScreen(false); };
            actions.NoteFallSpeedUp.started += _ => _stageContext.NoteFallSpeed += NoteFallSpeedDelta;
            actions.NoteFallSpeedDown.started += _ => _stageContext.NoteFallSpeed -= NoteFallSpeedDelta;
            actions.MusicSpeedUp.started += _ => _gamePlayContext.MusicSpeed += MusicSpeedDelta;
            actions.MusicSpeedDown.started += _ => _gamePlayContext.MusicSpeed -= MusicSpeedDelta;
        }

        #endregion

        #region NoteEdit

        private const float TimeDelta = 0.001f;
        private const float TimeDeltaLarge = 0.01f;
        private const float PositionDelta = 0.01f;
        private const float PositionDeltaLarge = 0.1f;
        private const float SizeDelta = 0.01f;
        private const float SizeDeltaLarge = 0.1f;
        private const float SpeedDelta = 0.01f;
        private const float SpeedDeltaLarge = 0.1f;
        private const float DurationDelta = 0.001f;
        private const float DurationDeltaLarge = 0.01f;

        private void RegisterNoteEdit()
        {
            var actions = _inputActions.NoteEdit;
            // actions.SelectAllNotes.started += _ => _ceditor.Selector.SelectAll();
            actions.RemoveSelectedNotes.started += _ => _editor.RemoveSelectedNotes();
            actions.Copy.started += _ => _ceditor.CopySelectedNotes();
            actions.Cut.started += _ => _ceditor.CutSelectedNotes();
            actions.Paste.started += _ => _ceditor.PasteNotes();
            actions.Redo.started += _ => _ceditor.OperationMemento.Redo(null);
            actions.Undo.started += _ => _ceditor.OperationMemento.Undo(null);
            actions.TimeDec.started += _ => _editor.EditNotesTime(_selection.SelectedNotes, t => t - TimeDelta);
            actions.TimeInc.started += _ => _editor.EditNotesTime(_selection.SelectedNotes, t => t + TimeDelta);
            actions.TimeDecLarge.started += _ => _editor.EditNotesTime(_selection.SelectedNotes, t => t - TimeDeltaLarge);
            actions.TimeIncLarge.started += _ => _editor.EditNotesTime(_selection.SelectedNotes, t => t + TimeDeltaLarge);
            actions.TimeDecByGrid.started += _ => _editor.EditNotesTime(_selection.SelectedNotes, t => _grids.TimeGrids.FloorToNearestNextGrid(t).Value ?? t);
            actions.TimeIncByGrid.started += _ => _editor.EditNotesTime(_selection.SelectedNotes, t => _grids.TimeGrids.CeilToNearestNextGrid(t).Value ?? t);
            actions.PositionLeft.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => p - PositionDelta);
            actions.PositionRight.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => p + PositionDelta);
            actions.PositionLeftLarge.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => p - PositionDeltaLarge);
            actions.PositionRightLarge.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => p + PositionDeltaLarge);
            actions.PositionLeftByGrid.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => _grids.PositionGrids.FloorToNearestNextGrid(p) ?? p);
            actions.PositionRightByGrid.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => _grids.PositionGrids.CeilToNearestNextGrid(p) ?? p);
            actions.PositionMirror.started += _ => _editor.EditNotesPosition(_selection.SelectedNotes, p => -p);
            actions.CoordQuantize.started += _ => _editor.EditPositionCoord(_selection.SelectedNotes, c => _grids.Quantize(c, true, true));
            actions.SizeDec.started += _ => _editor.EditNotesSize(_selection.SelectedNotes, s => s - SizeDelta);
            actions.SizeInc.started += _ => _editor.EditNotesSize(_selection.SelectedNotes, s => s + SizeDelta);
            actions.SizeDecLarge.started += _ => _editor.EditNotesSize(_selection.SelectedNotes, s => s - SizeDeltaLarge);
            actions.SizeIncLarge.started += _ => _editor.EditNotesSize(_selection.SelectedNotes, s => s + SizeDeltaLarge);
            actions.SpeedDec.started += _ => _editor.EditNotesSpeed(_selection.SelectedNotes, s => s -= SpeedDelta);
            actions.SpeedInc.started += _ => _editor.EditNotesSpeed(_selection.SelectedNotes, s => s += SpeedDelta);
            actions.SpeedDecLarge.started += _ => _editor.EditNotesSpeed(_selection.SelectedNotes, s => s -= SpeedDeltaLarge);
            actions.SpeedIncLarge.started += _ => _editor.EditNotesSpeed(_selection.SelectedNotes, s => s += SpeedDeltaLarge);
            actions.KindClick.started += _ => _editor.EditNotesKind(_selection.SelectedNotes, NoteKind.Click);
            actions.KindSlide.started += _ => _editor.EditNotesKind(_selection.SelectedNotes, NoteKind.Slide);
            actions.KindSwipe.started += _ => _editor.EditNotesKind(_selection.SelectedNotes, NoteKind.Swipe);
            actions.SoundAdd.started += _ => _editor.EditNotesSounds(_selection.SelectedNotes, true);
            actions.SoundRemove.started += _ => _editor.EditNotesSounds(_selection.SelectedNotes, false);
            actions.DurationDec.started += _ => _editor.EditNotesDuration(_selection.SelectedNotes, d => d - DurationDelta);
            actions.DurationInc.started += _ => _editor.EditNotesDuration(_selection.SelectedNotes, d => d + DurationDelta);
            actions.DurationDecLarge.started += _ => _editor.EditNotesDuration(_selection.SelectedNotes, d => d - DurationDeltaLarge);
            actions.DurationIncLarge.started += _ => _editor.EditNotesDuration(_selection.SelectedNotes, d => d + DurationDeltaLarge);
            actions.DurationDecByGrid.started += _ => _editor.EditNotesEndTime(_selection.SelectedNotes, t => _grids.TimeGrids.FloorToNearestNextGrid(t).Value ?? t);
            actions.DurationIncByGrid.started += _ => _editor.EditNotesEndTime(_selection.SelectedNotes, t => _grids.TimeGrids.CeilToNearestNextGrid(t).Value ?? t);
            actions.CreateHoldBetween.started += _ =>
            {
                if (_selection.SelectedNotes.Length != 2)
                    return;

                var prev = _selection.SelectedNotes[0];
                var next = _selection.SelectedNotes[1];

                _editor.CreateHoldBetween(prev, next);
            };
        }

        #endregion

        #region EditorSettings

        private void RegisterEditorSettings()
        {
            var actions = _inputActions.EditorSettings;
            actions.SnapToGrids.started += _ =>
            {
                var placer = _ceditor.Placer;
                var val = !(placer.SnapToPositionGrid && placer.SnapToTimeGrid);
                placer.SnapToPositionGrid = placer.SnapToTimeGrid = val;
            };
            actions.PasteRememberPosition.started += _ => _ceditor.Placer.PasteRememberPositionModifier = true;
            actions.PasteRememberPosition.canceled += _ => _ceditor.Placer.PasteRememberPositionModifier = false;
            actions.PlaceNoteSlideFlag.started += _ => _ceditor.Placer.PlaceSlideModifier = true;
            actions.PlaceNoteSlideFlag.canceled += _ => _ceditor.Placer.PlaceSlideModifier = false;
            actions.PlaceSoundNote.started += _ => _ceditor.Placer.PlaceSoundNoteByDefault = !_ceditor.Placer.PlaceSoundNoteByDefault;
        }

        #endregion

        #region Mouse Action / NotePlacement

        private void Update_MouseAction()
        {
            //if (_game.IsChartLoaded() && _game.IsStageLoaded()) {
            //    //var mouse = Mouse.current;
            //    //var pos = mouse.position.ReadValue();

            //    //if (MainWindow.Views.PerspectiveViewPanelView.IsHovering) {
            //    //    if (mouse.leftButton.wasPressedThisFrame)
            //    //        _mouseEditing.LeftMouseDown(pos);
            //    //    //OnLeftMouseDown(pos);
            //    //    if (mouse.rightButton.wasPressedThisFrame)
            //    //        _mouseEditing.RightMouseDown(pos);
            //    //    //OnRightMouseDown(pos);
            //    //    if (mouse.leftButton.wasReleasedThisFrame)
            //    //        _mouseEditing.LeftMouseUp(pos);
            //    //    //OnLeftMouseUp(pos);
            //    //    if (mouse.rightButton.wasReleasedThisFrame)
            //    //        _mouseEditing.RightMouseUp(pos);
            //    //    //OnRightMouseUp(pos);
            //    //}
            //    //_mouseEditing.MouseMove(pos);
            //    ////OnMouseMove(pos);
            //}
        }

        //private void OnLeftMouseDown(Vector2 mousePosition)
        //{
        //    if (_ceditor.Placer.IsPlacing) {
        //        _ceditor.Placer.CancelPlaceNote();
        //    }
        //    else {
        //        if (TryConvertScreenPointToNoteCoord(mousePosition, false, out var coord)) {
        //            _ceditor.Selector.BeginDragSelect(coord, toggleMode: UnityUtils.IsFunctionalKeyHolding(ctrl: true));
        //        }
        //    }
        //}

        //private void OnRightMouseDown(Vector2 mousePosition)
        //{
        //    if (_ceditor.Selector.IsDragSelecting)
        //        return;
        //    else {
        //        if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
        //            _ceditor.Placer.BeginPlaceNote(coord, mousePosition);
        //        }
        //    }
        //}

        //private void OnMouseMove(Vector2 mousePosition)
        //{
        //    if (_ceditor.Selector.IsDragSelecting) {
        //        if (TryConvertScreenPointToNoteCoord(mousePosition, false, out var coord)) {
        //            _ceditor.Selector.UpdateDragSelect(coord);
        //        }
        //    }
        //    else {
        //        if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
        //            _ceditor.Placer.UpdatePlaceNote(coord, mousePosition);
        //        }
        //        else {
        //            _ceditor.Placer.DisablePlaceNote();
        //        }
        //    }
        //}

        //private void OnLeftMouseUp(Vector2 mousePosition)
        //{
        //    if (_ceditor.Selector.IsDragSelecting) {
        //        if (MainWindow.Views.PerspectiveViewPanelView.TryConvertScreenPointToViewportPoint(mousePosition, out var vp))
        //            _ceditor.Selector.EndDragSelect(vp);
        //    }
        //}

        //private void OnRightMouseUp(Vector2 mousePosition)
        //{
        //    if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
        //        _ceditor.Placer.EndPlaceNote(coord, mousePosition);
        //    }
        //    else {
        //        _ceditor.Placer.CancelPlaceNote();
        //    }
        //}

        //private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, bool applyHighlightNoteSpeed, out NoteCoord coord)
        //{
        //    MainSystem.GamePlayManager.AssertStageLoaded();

        //    if (!MainWindow.Views.PerspectiveViewPanelView.TryConvertScreenPointToViewportPoint(screenPoint, out var viewPoint)) {
        //        coord = default;
        //        return false;
        //    }

        //    var res = MainSystem.GamePlayManager.TryConvertPerspectiveViewportPointToNoteCoord(viewPoint,
        //        applyHighlightNoteSpeed ? MainSystem.StageChartEditor.Placer.PlacingNoteSpeed : 1f, out coord);

        //    return res;
        //}

        #endregion

        #region ProjectManage

        private void RegisterProjectManagement()
        {
            var actions = _inputActions.ProjectManagement;
            actions.NewProject.started += _ => MainWindow.Views.MenuNavigationPageView.MenuCreateNewProjectAsync().Forget();
            actions.OpenProject.started += _ => MainWindow.Views.MenuNavigationPageView.MenuOpenProjectAsync().Forget();
            actions.SaveProject.started += _ => MainWindow.Views.MenuNavigationPageView.MenuSaveProjectAsync().Forget();
            actions.SaveProjectAs.started += _ => MainWindow.Views.MenuNavigationPageView.MenuSaveProjectAsAsync().Forget();
            // TODO: ExportChartJsons Impl

        }

        #endregion
    }
}