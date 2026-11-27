using Deenote.CoreB;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.GameStage.UI;
using Deenote.Project;
using Deenote.Systems;
using System;
using UnityEngine;

namespace Deenote.Inputting
{
    public sealed class InputInterpreter
    {
        private KeyboardShortcutInputActions _shortcuts = default!;
        private PlacementCoordinator _placementCoordinator = default!;

        private GamePlayManager2 _gamePlayManager = default!;
        private GameStageManager _gameStageManager = default!;
        private ProjectManager2 _projectManager = default!;
        private EditingManager _editingManager = default!;
        private EnvironmentContext _environment = default!;

        private IMouseInputHandler _mouseInputHandler = default!;
        private IGameStagePerspectiveViewPanel _stagePanel = default!;

        public InputInterpreter(
            GamePlayManager2 gamePlayManager,
            GameStageManager gameStageManager,
            ProjectManager2 projectManager,
            EditingManager editingManager,
            EnvironmentContext environment,
            IMouseInputHandler mouseInputHandler,
            IGameStagePerspectiveViewPanel stagePanel)
        {
            _gamePlayManager = gamePlayManager;
            _gameStageManager = gameStageManager;
            _projectManager = projectManager;
            _editingManager = editingManager;
            _environment = environment;

            _mouseInputHandler = mouseInputHandler;
            _stagePanel = stagePanel;

            _shortcuts = new();
            _placementCoordinator = new(_gamePlayManager, _gameStageManager, _editingManager, stagePanel);

            RegisterGamePlayShortcuts();
            RegisterStageSettingsShortcuts();
            RegisterNoteEditShortcuts();
            RegisterEditorSettingsShortcuts();

            _mouseInputHandler.Pressed += e =>
            {
                if (e.LeftMouse) {
                    _placementCoordinator.LeftMouseDown(e.ScreenPoint);
                }
                if (e.RightMouse) {
                    _placementCoordinator.RightMouseDown(e.ScreenPoint);
                }
            };
            _mouseInputHandler.Released += e =>
            {
                if (e.LeftMouse) {
                    _placementCoordinator.LeftMouseUp(e.ScreenPoint);
                }
                if (e.RightMouse) {
                    _placementCoordinator.RightMouseUp(e.ScreenPoint);
                }
            };
            _mouseInputHandler.Moved += e =>
            {
                _placementCoordinator.MouseMove(e.ScreenPoint);
            };

            _projectManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    SetGamePlayEnabled();
                    SetCoordinatorEnabled();
                }
            });
            _gameStageManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.GameStage))) {
                    SetGamePlayEnabled();
                    SetCoordinatorEnabled();
                }
            });

            App.Current.UnscaledTick += e =>
            {
                _placementCoordinator.Tick();
            };
        }

        private void SetCoordinatorEnabled()
        {
            _placementCoordinator.SetEnabled(_projectManager.CurrentChart is not null && _gameStageManager.GameStage is not null);
        }

        #region GamePlay

        private void SetGamePlayEnabled()
        {
            if (_projectManager.CurrentChart is not null && _gameStageManager.GameStage is not null) {
                _shortcuts.StageGamePlay.Enable();
            }
            else {
                _shortcuts.StageGamePlay.Disable();
            }
        }

        private float? _musicResetTime;
        private float? _manualPlaySpeed;
        private bool _manualPlaySpeedUp;

        private void RegisterGamePlayShortcuts()
        {
            var actions = _shortcuts.StageGamePlay;

            actions.PauseResume.started += _ => _gamePlayManager.TogglePlayingState();
            actions.AutoResetPlay.started += _ =>
            {
                _musicResetTime = _gamePlayManager.CurrentTime;
                _gamePlayManager.Play();
            };
            actions.AutoResetPlay.canceled += _ =>
            {
                if (_musicResetTime is { } mrt) {
                    _gamePlayManager.Stop();
                    _gamePlayManager.CurrentTime = mrt;
                }
            };
            actions.ToMusicStart.started += _ => _gamePlayManager.CurrentTime = 0;
            actions.ToMusicEnd.started += _ => _gamePlayManager.CurrentTime = _gamePlayManager.GamePlayLength;
            actions.ManualPlay.started += ctx =>
            {
                _manualPlaySpeed = ctx.ReadValue<float>();
                PerformManualPlay();
            };
            actions.ManualPlay.canceled += ctx =>
            {
                _manualPlaySpeed = null;
                PerformManualPlay();
            };
            actions.ManualPlaySpeedUp.started += _ =>
            {
                _manualPlaySpeedUp = true;
                if (_manualPlaySpeed is not null)
                    PerformManualPlay();
            };
            actions.ManualPlaySpeedUp.canceled += _ =>
            {
                _manualPlaySpeedUp = false;
                if (_manualPlaySpeed is not null)
                    PerformManualPlay();
            };
            actions.ScrollPlay.performed += ctx =>
            {
                if (!_mouseInputHandler.IsHovering)
                    return;

                var delta = ctx.ReadValue<Vector2>().y;
                if (delta != 0f) {
                    var deltaTime = -delta * 0.001f * MainSystem.GlobalSettings.GameViewScrollSensitivity;
                    App.GamePlayManager.Nudge(deltaTime);
                }
            };

            void PerformManualPlay()
            {
                var multiplier = _manualPlaySpeedUp ? 5.0f : 2.5f;
                _gamePlayManager.SetManualPlaySpeed(multiplier * _manualPlaySpeed);
            }
        }

        #endregion

        #region StageSettings

        private void RegisterStageSettingsShortcuts()
        {
            const int NoteFallSpeedDelta = 5;
            const int MusicSpeedDelta = 1;

            var actions = _shortcuts.StageSettings;

            actions.EscapeFullScreen.started += _ => _stagePanel.IsFullScreen = false;
            actions.NoteFallSpeedUp.started += _ => _gameStageManager.NoteFallSpeed += NoteFallSpeedDelta;
            actions.NoteFallSpeedDown.started += _ => _gameStageManager.NoteFallSpeed -= NoteFallSpeedDelta;
            actions.MusicSpeedUp.started += _ => _gamePlayManager.MusicSpeed += MusicSpeedDelta;
            actions.MusicSpeedDown.started += _ => _gamePlayManager.MusicSpeed -= MusicSpeedDelta;
        }

        #endregion

        #region NoteEdit

        private void RegisterNoteEditShortcuts()
        {
            var actions = _shortcuts.NoteEdit;
            actions.SelectAllNotes.started += _ => { };
            actions.RemoveSelectedNotes.started += _ => { };
            actions.Copy.started += _ => _editingManager.NotesClipBoard.SetClipBoard(_editingManager.NotesSelector.SelectedNotes.AsSpan());
            actions.Cut.started += _ =>
            {
                if (_projectManager.CurrentChart is null)
                    return;

                _editingManager.NotesClipBoard.SetClipBoard(_editingManager.NotesSelector.SelectedNotes.AsSpan());
                _editingManager.ChartEditor.RemoveNotes(_projectManager.CurrentChart, _editingManager.NotesSelector.SelectedNotes.AsSpan());
            };
            actions.Paste.started += _ => _placementCoordinator.PreparePaste();
            actions.Redo.started += _ => { };
            actions.Undo.started += _ => { };
            actions.TimeDec.started += _ => { };
            actions.TimeInc.started += _ => { };
            actions.TimeDecLarge.started += _ => { };
            actions.TimeIncLarge.started += _ => { };
            actions.TimeDecByGrid.started += _ => { };
            actions.TimeIncByGrid.started += _ => { };
            actions.PositionLeft.started += _ => { };
            actions.PositionRight.started += _ => { };
            actions.PositionLeftLarge.started += _ => { };
            actions.PositionRightLarge.started += _ => { };
            actions.PositionLeftByGrid.started += _ => { };
            actions.PositionRightByGrid.started += _ => { };
            actions.PositionMirror.started += _ => { };
            actions.CoordQuantize.started += _ => { };
            actions.SizeDec.started += _ => { };
            actions.SizeInc.started += _ => { };
            actions.SizeDecLarge.started += _ => { };
            actions.SizeIncLarge.started += _ => { };
            actions.SpeedDec.started += _ => { };
            actions.SpeedInc.started += _ => { };
            actions.SpeedDecLarge.started += _ => { };
            actions.SpeedIncLarge.started += _ => { };
            actions.KindClick.started += _ => { };
            actions.KindSlide.started += _ => { };
            actions.KindSwipe.started += _ => { };
            actions.SoundAdd.started += _ => { };
            actions.SoundRemove.started += _ => { };
            actions.DurationDec.started += _ => { };
            actions.DurationInc.started += _ => { };
            actions.DurationDecLarge.started += _ => { };
            actions.DurationIncLarge.started += _ => { };
            actions.DurationDecByGrid.started += _ => { };
            actions.DurationIncByGrid.started += _ => { };
            actions.CreateHoldBetween.started += _ => { };
        }

        #endregion

        #region EditorSettings

        private void RegisterEditorSettingsShortcuts()
        {
            var actions = _shortcuts.EditorSettings;
            actions.SnapToGrids.started += _ =>
            {
                var placer = _editingManager.NotesPlacer;
                var val = !(placer.SnapToPositionGrids && placer.SnapToTimeGrids);
                placer.SnapToPositionGrids = val;
                placer.SnapToTimeGrids = val;
            };
            actions.PasteRememberPosition.started += _ => _editingManager.NotesPlacer.IsPastingRememberPosition = true;
            actions.PasteRememberPosition.canceled += _ => _editingManager.NotesPlacer.IsPastingRememberPosition = false;
            actions.PlaceNoteSlideFlag.started += _ => _editingManager.NotesPlacer.IsPlacingSlideRequested = true;
            actions.PlaceNoteSlideFlag.canceled += _ => _editingManager.NotesPlacer.IsPlacingSlideRequested = false;
            actions.PlaceSoundNote.started += _ => _editingManager.NotesPlacer.PlaceSpeedWarningNote = !_editingManager.NotesPlacer.PlaceSpeedWarningNote;
        }

        #endregion
    }
}