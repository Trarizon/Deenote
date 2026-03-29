#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Api.Operations;
using Deenote.Contexts;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Editing.Operations;
using Deenote.Library.Components;
using Deenote.ProjectManagement;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed partial class StageChartEditor : FlagNotifiableMonoBehaviour<StageChartEditor, StageChartEditor.NotificationFlag>
    {
        private ProjectContext _projectContext = default!;

        internal ProjectManager _project = default!;
        internal GamePlayManager _game = default!;

        private OperationMemento _operations = default!;

        public StageNotePlacer Placer { get; private set; } = default!;
        public StageNoteSelector Selector { get; private set; } = default!;
        public OperationMemento OperationMemento => _operations;

        private void Awake()
        {
            _projectContext = MainSystem.Contexts.Project;
            _operations = new();

            Awake_ClipBoard();

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Set("editor/indicator", Placer.IsIndicatorOn);
                configs.Set("editor/snap_pos", Placer.SnapToPositionGrid);
                configs.Set("editor/snap_time", Placer.SnapToTimeGrid);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                Placer.IsIndicatorOn = configs.GetBoolean("editor/indicator", true);
                Placer.SnapToPositionGrid = configs.GetBoolean("editor/snap_pos", true);
                Placer.SnapToTimeGrid = configs.GetBoolean("editor/snap_time", true);
            };
            MainSystem.ProjectManagerB.ProjectSaved += () =>
            {
                OperationMemento.SaveAtCurrent();
            };

            _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentProject))) {
                    _operations.Reset();
                }
            });
        }

        internal void OnInstantiate(ProjectManager project, GamePlayManager game)
        {
            _project = project;
            _game = game;

            Placer = new StageNotePlacer(this);
            Selector = new StageNoteSelector(game);

            //_project.RegisterNotification(
            //    ProjectManager.NotificationFlag.CurrentProject,
            //    manager =>
            //    {
            //        _operations.Reset();
            //    });

            /*
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager =>
                {
                    _operations.Reset();
                });
            */
        }

        #region Add Remove

        private void OnNoteCollectionChanged()
        {
            _game.AssertChartLoaded();
            NoteComparers.AssertInOrderViaTime(_game.CurrentChart.Notes);
            _game.UpdateNotes(true, false);
        }

        public void AddNote(NoteData note)
        {
            if (!_game.IsChartLoaded())
                return;

            _operations.Do(_game.CurrentChart.GetAddNoteOperation(note)
                .OnRedone(note =>
                {
                    this.Selector.Clear();
                    OnNoteCollectionChanged();
                    ModelAsserts.AssertChartEditorModel(_game.CurrentChart);
                })
                .OnUndone(note => OnNoteCollectionChanged()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="baseCoord">The coord that first note will be created</param>
        public void AddMultipleNotes(ReadOnlySpan<NoteData> notes)
        {
            if (!_game.IsChartLoaded())
                return;
            if (notes.IsEmpty)
                return;
            if (notes.Length == 1) {
                AddNote(notes[0]);
                return;
            }

            _operations.Do(_game.CurrentChart.GetAddNotesOperation(notes)
                .OnRedone(notes =>
                {
                    this.Selector.Reselect(notes);
                    OnNoteCollectionChanged();
                })
                .OnUndone(notes =>
                {
                    this.Selector.DeselectMultiple(notes);
                    OnNoteCollectionChanged();
                    ModelAsserts.AssertChartEditorModel(_game.CurrentChart);
                }));
        }

        public void RemoveNotes(ReadOnlySpan<NoteEditorModel> notes)
        {
            if (!_game.IsChartLoaded())
                return;
            if (notes.IsEmpty)
                return;

            _operations.Do(GetRemoveNotesOperation(notes));
        }

        public IOperation GetRemoveNotesOperation(ReadOnlySpan<NoteEditorModel> notes)
        {
            _game.AssertChartLoaded();

            return _game.CurrentChart.GetRemoveNotesOperation(notes.ToImmutableArray())
                .OnRedone(notes =>
                {
                    this.Selector.Clear();
                    OnNoteCollectionChanged();
                })
                .OnUndone(notes =>
                {
                    this.Selector.AddSelectMultiple(notes);
                    OnNoteCollectionChanged();
                });
        }

        public void RemoveSelectedNotes() => RemoveNotes(Selector.SelectedNotes);

        public void AddNotesSnappingToCurve(int count, ReadOnlySpan<GridsManager.CurveApplyProperty> applyProperties = default)
        {
            if (!_game.IsChartLoaded())
                return;
            if (_game.Grids.CurveTimeInterval is not (var start, var end))
                return;

            bool applySize = false, applySpeed = false;
            foreach (var apply in applyProperties) {
                if (apply is GridsManager.CurveApplyProperty.Size)
                    applySize = true;
                if (apply is GridsManager.CurveApplyProperty.Speed)
                    applySpeed = true;
            }

            using var so_notes = SpanOwner<NoteData>.Allocate(count);
            var notes = so_notes.Span;
            for (int i = 0; i < count; i++) {
                var time = Mathf.Lerp(start, end, (float)(i + 1) / (count + 1));
                var pos = _game.Grids.GetCurveTransformedPosition(time)!.Value; // Wont be null as CurveTimeInterval is not null
                var note = notes[i] = Placer.ClonePlaceNotePrototype();
                note.PositionCoord = new(pos, time);
                if (applySize)
                    note.Size = _game.Grids.GetCurveTransformedValue(time, GridsManager.CurveApplyProperty.Size)!.Value;
                if (applySpeed)
                    note.Speed = _game.Grids.GetCurveTransformedValue(time, GridsManager.CurveApplyProperty.Speed)!.Value;
            }
            AddMultipleNotes(notes);
        }

        #endregion

        public enum NotificationFlag
        {
            NoteTime,
            NotePosition,
            NotePositionCoord,
            NoteSize,
            NoteShift,
            NoteSpeed,
            NoteDuration,
            NoteKind,
            NoteVibrate,
            NoteWarningType,
            NoteEventId,
            NoteSounds,

            ProjectTempo,
        }
    }
}