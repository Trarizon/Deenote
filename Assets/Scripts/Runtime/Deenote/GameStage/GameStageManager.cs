#nullable enable

using Deenote.Contexts;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using System;
using UnityEngine;

namespace Deenote.GameStage
{
    internal sealed class GameStageManager
    {
        private readonly GameStageContext _context;
        private readonly GameStageNotesManager _notesManager;
        private readonly GameStageGridsManager _gridsManager;
        private readonly GameStagePlacementIndicatorsManager _indicatorsManager;
        private readonly InputInterpreter _inputInterpreter;

        public GameStageManager(GameStageContext context, ProjectContext project, EditorContext editor, GamePlayContext gamePlay, GameStageThemeManager themeManager, InputInterpreter inputInterpreter)
        {
            _context = context;
            _notesManager = new(context, project);
            _gridsManager = new(editor.Grids, gamePlay, context);
            _indicatorsManager = new(editor.NotePlacement);
            _inputInterpreter = inputInterpreter;
        }

        internal void OnStart()
        {
            _context.ThemeContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentTheme))) {
                    if (s.CurrentTheme is null)
                        return;
                    if (_context.GameStage is null)
                        return;
                    try {
                        _context.GameStage.Initialize(_context);
                        _context.NotesContext.RefreshActiveVisibleNotes();
                        _notesManager.Initialize(s.CurrentTheme.NoteFactory);
                        _indicatorsManager.Initialize(s.CurrentTheme.NoteFactory);
                        _gridsManager.SetLinesRenderer(_context.GameStage.PerspectiveLinesRenderer);
                    } catch (Exception ex) {
                        Debug.LogError(ex.Message + ex.StackTrace);
                        throw;
                    }
                }
            });

            RegisterInputs();
        }

        private void RegisterInputs()
        {
            const int NoteFallSpeedDelta = 5;

            var actions = _inputInterpreter.InputActions.StageSettings;
            actions.NoteFallSpeedUp.started += _ => _context.NoteFallSpeed += NoteFallSpeedDelta;
            actions.NoteFallSpeedDown.started += _ => _context.NoteFallSpeed -= NoteFallSpeedDelta;
        }
    }
}
