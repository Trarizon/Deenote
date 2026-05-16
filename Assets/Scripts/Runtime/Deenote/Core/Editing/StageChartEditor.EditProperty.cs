#nullable enable

using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Editing.Operations;
using System;

namespace Deenote.Core.Editing
{
    partial class StageChartEditor
    {
        #region Simple Edit Note Properties

        internal static readonly PianoSoundData[] _defaultNoteSounds
            = new[] { new PianoSoundData(0f, 0f, 72, 0) };

        //private void OnNotePropertyEdited(bool notesVerticalPositionChanged, bool notesVisualDataChanged, NotificationFlag flag)
        //{
        //    _game.AssertChartLoaded();
        //    ModelAsserts.AssertChartEditorModel(_game.CurrentChart);
        //    NotifyFlag(flag);
        //    _game.UpdateNotes(notesVerticalPositionChanged, notesVisualDataChanged);
        //}

        [Obsolete("Builtin for chart concatenation, this may be changed removed in the future")]
        public void ConcatNotes(ChartData other, float offset, float multiplier)
        {
            if (_projectContext.CurrentChart is  null)
                return;
            _operations.Do(_projectContext.CurrentChart!
                .GetConcatChartOperation(other, offset, multiplier)
                .OnRedone(_ => _game.UpdateNotes(true, true))
                .OnUndone(_ => _game.UpdateNotes(true, true)));
        }

        //[Obsolete]
        //public void EditSelectedNotesPositionCoord(Func<NoteCoord, NoteCoord> valueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    float clipLength = _game.MusicPlayer.ClipLength;
        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesCoordOperation(Selector.SelectedNotes.ToImmutableArray(), v => NoteCoord.Clamp(valueSelector(v), clipLength))
        //        .OnDone(notes => OnNotePropertyEdited(true, true, NotificationFlag.NotePositionCoord)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesTime(Func<float, float> valueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    float clipLength = _game.MusicPlayer.ClipLength;
        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesTimeOperation(Selector.SelectedNotes.ToImmutableArray(), v => NoteConstraints.ClampTime(valueSelector(v), clipLength))
        //        .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteTime)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesTime(float newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    float clipLength = _game.MusicPlayer.ClipLength;
        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesTimeOperation(Selector.SelectedNotes.ToImmutableArray()    , NoteConstraints.ClampTime(newValue, clipLength))
        //        .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteTime)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesPosition(Func<float, float> valueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesPositionOperation(Selector.SelectedNotes.ToImmutableArray(), v => NoteConstraints.ClampPosition(valueSelector(v)))
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NotePosition)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesPosition(float newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesPositionOperation(Selector.SelectedNotes.ToImmutableArray(), NoteConstraints.ClampPosition(newValue))
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NotePosition)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesSize(Func<float, float> valueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), v => NoteConstraints.ClampSize(valueSelector(v)),
        //            n => n.Size, (n, v) => n.Size = v)
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSize)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesSize(float newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), NoteConstraints.ClampSize(newValue),
        //            n => n.Size, (n, v) => n.Size = v)
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSize)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesShift(float newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), newValue,
        //            n => n.Shift, (n, v) => n.Shift = v)
        //        .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteShift)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesSpeed(Func<float, float> valueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), v => NoteConstraints.ClampSpeed(valueSelector(v)),
        //            n => n.Speed, (n, v) => n.Speed = v)
        //        .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteSpeed)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesSpeed(float newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), NoteConstraints.ClampSpeed(newValue),
        //            n => n.Speed, (n, v) => n.Speed = v)
        //        .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteSpeed)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesDuration(Func<float, float> valueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesDurationOperation(Selector.SelectedNotes.ToImmutableArray(), v => Mathf.Max(0, valueSelector(v)))
        //        .OnDone(notes => OnNotePropertyEdited(true, true, NotificationFlag.NoteDuration)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesEndTime(Func<float, float> newValueSelector)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesEndTimeOperation(Selector.SelectedNotes.ToImmutableArray(), v => Mathf.Max(0, newValueSelector(v)))
        //        .OnDone(notes => OnNotePropertyEdited(true, true, NotificationFlag.NoteDuration)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesDuration(float newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(GetEditNotesDurationOperation(Selector.SelectedNotes, newValue));
        //}

        //[Obsolete]
        //private IOperation GetEditNotesDurationOperation(ReadOnlySpan<NoteEditorModel> notes, float newValue)
        //{
        //    _game.AssertChartLoaded();

        //    return _game.CurrentChart
        //        .GetEditNotesDurationOperation(notes.ToImmutableArray(), newValue)
        //        .OnDone(notes => OnNotePropertyEdited(true, true, NotificationFlag.NoteDuration));
        //}

        //[Obsolete]
        //public void EditSelectedNotesVibrate(bool newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), newValue,
        //            n => n.Vibrate, (n, v) => n.Vibrate = v)
        //        .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteVibrate)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesKind(NoteKind newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesKindOperation(Selector.SelectedNotes.ToImmutableArray(), newValue)
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteKind)));
        //}

        //[Obsolete]
        //private void EditSelectedNotesWarningType(WarningType newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), newValue,
        //            n => n.WarningType, (n, v) => n.WarningType = v)
        //        .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteWarningType)));
        //}

        //[Obsolete]
        //public void EditSelectedNotesEventId(string newValue)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesOperation(Selector.SelectedNotes.ToImmutableArray(), newValue,
        //            n => n.EventId, (n, v) => n.EventId = v)
        //        .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteEventId)));
        //}

        //[Obsolete]
        //public void EditSelectedNoteSounds(ReadOnlySpan<PianoSoundData> values)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesSoundsOperation(Selector.SelectedNotes.ToImmutableArray(), values.ToImmutableArray())
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSounds)));
        //}

        ///// <summary>
        ///// Quick add or remove sound, if <paramref name="hasSound"/> but note already has sounds,
        ///// do nothing.
        ///// </summary>
        //[Obsolete]
        //public void EditSelectedNoteSounds(bool hasSound)
        //{
        //    if (!_game.IsChartLoaded())
        //        return;
        //    if (Selector.SelectedNotes.IsEmpty)
        //        return;

        //    using var so_editNotes = SpanOwner<NoteEditorModel>.Allocate(Selector.SelectedNotes.Length);
        //    var editNotes = so_editNotes.Span;
        //    int index = 0;
        //    foreach (var note in Selector.SelectedNotes) {
        //        if (note.HasSounds != hasSound)
        //            editNotes[index++] = note;
        //    }

        //    _operations.Do(_game.CurrentChart
        //        .GetEditNotesSoundsOperation(editNotes[..index].ToImmutableArray(), hasSound ? _defaultNoteSounds.ToImmutableArray() : default)
        //        .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSounds)));
        //}

        #endregion

        //[Obsolete]
        //private void ApplySelectedNotesWithCurveTranform(GridsManager.CurveApplyProperty property)
        //{
        //    //switch (property) {
        //    //    case GridsManager.CurveApplyProperty.Size:
        //    //        EditSelectedNotesSize(v => _context.Grids.Curves.SizeCurve?.GetValue(v) ?? v);
        //    //        break;
        //    //    case GridsManager.CurveApplyProperty.Speed:
        //    //        EditSelectedNotesSpeed(v => _context.Grids.Curves.SpeedCurve?.GetValue(v) ?? v);
        //    //        break;
        //    //    default:
        //    //        ThrowHelper.ThrowInvalidOperationException("Unknown curve apply property");
        //    //        break;
        //    //}
        //}

        //[Obsolete]
        //public void CreateHoldBetween(NoteEditorModel head, NoteEditorModel tail)
        //{
        //    if (tail.Time == head.Time)
        //        return;

        //    if (tail.Time < head.Time)
        //        (head, tail) = (tail, head);

        //    var duration = tail.Time - head.Time;
        //    var rmv = GetRemoveNotesOperation(MemoryMarshal.CreateReadOnlySpan(ref tail, 1));
        //    var duredit = GetEditNotesDurationOperation(MemoryMarshal.CreateReadOnlySpan(ref head, 1), duration);
        //    OperationMemento.Do(new CombinedPairOperation(rmv, duredit));
        //}

        //[Obsolete]
        //public void InsertTempo(TempoRange range)
        //{
        //    if (!_project.IsProjectLoaded())
        //        return;

        //    _operations.Do(_project.CurrentProject.InsertTempo(range)
        //        .OnDone(() => NotifyFlag(NotificationFlag.ProjectTempo)));
        //}
    }
}