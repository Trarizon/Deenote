#nullable enable

using Deenote.Core.GamePlay;
using Deenote.CoreB.Models;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Library.Collections;
using Deenote.Library.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Core.Editing
{
    [Obsolete]
    public sealed class StageNoteSelector
    {
        private const float DragSelectionAreaMaxPosition = 6f;

        // private GamePlayManager _game = default!;

        private readonly List<NoteEditorModel> _selectedNotes = new();

        private NoteCoord _dragStartCoord;
        private NoteCoord _dragEndCoord;
        private readonly List<NoteEditorModel> _inDragRangeNotes = new();
        private DraggingSelectionState _state;

        public bool IsDragSelecting => _state != DraggingSelectionState.Idle;

        public ReadOnlySpan<NoteEditorModel> SelectedNotes => default!; // _selectedNotes.AsSpan();

        public event Action<StageNoteSelector>? SelectedNotesChanging;
        public event Action<StageNoteSelector>? SelectedNotesChanged;

        // internal StageNoteSelector(GamePlayManager game)
        // {
        //     _game = game;
        //     //_game.RegisterNotification(
        //     //    GamePlayManager.NotificationFlag.CurrentChart,
        //     //    manager => Clear());
        //     //_game.MusicPlayer.TimeChanged += args =>
        //     //{
        //     //    if (!IsDragSelecting)
        //     //        return;

        //     //    var delta = args.NewTime - args.OldTime;
        //     //    _dragEndCoord.Time += delta;
        //     //    UpdateDragSelection(_dragStartCoord, _dragEndCoord);
        //     //};
        // }

        private enum DraggingSelectionState
        {
            Idle,
            StartedSelection,
            Dragging,
        }
    }
}