#nullable enable

using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine.Pool;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class ChartEditorModel
    {
        public List<NoteEditorModel> Notes { get; private set; }
        internal List<IGameStageNoteNode> NoteNodes { get; private set; }
        internal List<BackgroundNoteEditorModel> BackgroundNotes { get; private set; }
        internal List<WarningNoteEditorModel> WarningNotes { get; private set; }

        //internal Dictionary<ICollidableNote, List<ICollidableNote>> Collisions { get; } = new();

        // Events

        public event CollectionChangeEventHandler<ChartEditorModel, NoteEditorModel>? NotesChanged;

        // Modifications

        private void AddNoteNonNotify(NoteEditorModel note)
        {
            NoteCollisionHelpers.UpdateCollisionsPreAdding(this, note);

            Notes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note);
            NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note);
            if (note.Tail is not null) {
                NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Add(note.Tail);
            }
        }

        internal void AddNoteEditorModel(NoteEditorModel note)
        {
            AddNoteNonNotify(note);
            NotesChanged?.Invoke(this, CollectionChangedEventArgs.Add(MemoryMarshal.CreateReadOnlySpan(ref note, 1)));
        }

        internal void AddNoteEditorModels(ReadOnlySpan<NoteEditorModel> notes)
        {
            // Optimize
            foreach (var note in notes) {
                AddNoteNonNotify(note);
            }
            NotesChanged?.Invoke(this, CollectionChangedEventArgs.Add(notes));
        }

        private void RemoveNoteNonNotify(NoteEditorModel note)
        {
            Notes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note);
            NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note);
            if (note.Tail is not null) {
                NoteNodes.GetSortedModifier(ModelComparers.ViaTimeUnique).Remove(note.Tail);
            }

            NoteCollisionHelpers.UpdateCollisionsPostRemoving(this, note);
        }

        internal void RemoveNoteEditorModel(NoteEditorModel note)
        {
            RemoveNoteNonNotify(note);
            NotesChanged?.Invoke(this, CollectionChangedEventArgs.Remove(MemoryMarshal.CreateReadOnlySpan(ref note, 1)));
        }

        internal void RemoveNoteEditorModels(ReadOnlySpan<NoteEditorModel> notes)
        {
            // Optimize
            foreach (var note in notes) {
                RemoveNoteNonNotify(note);
            }
            NotesChanged?.Invoke(this, CollectionChangedEventArgs.Remove(notes));
        }

        // Other

        [MemberNotNull(nameof(Notes), nameof(NoteNodes), nameof(BackgroundNotes), nameof(WarningNotes))]
        private void CloneNotes(ChartModel model)
        {
            using var dp_linkLookup = DictionaryPool<NoteData, INoteLink>.Get(out var linkLookup);

            var notes = new List<NoteEditorModel>();
            notes.EnsureCapacity(model.Notes.Count);
            foreach (var note in model.Notes) {
                var editorModel = new NoteEditorModel(note);
                linkLookup.Add(note, editorModel);
                notes.Add(editorModel);
            }

            var backgrounds = new List<BackgroundNoteEditorModel>();
            backgrounds.EnsureCapacity(model.BackgroundNotes.Count);
            foreach (var note in model.BackgroundNotes) {
                var editorModel = new BackgroundNoteEditorModel(note);
                linkLookup.Add(note.GetUnderlyingData(), editorModel);
                backgrounds.Add(editorModel);
            }

            var warnings = new List<WarningNoteEditorModel>();
            warnings.EnsureCapacity(model.WarningNotes.Count);
            foreach (var note in model.WarningNotes) {
                var editorModel = new WarningNoteEditorModel(note);
                linkLookup.Add(note.GetUnderlyingData(), editorModel);
                warnings.Add(editorModel);
            }

            foreach (var note in model.Notes) {
                if (note.NextLink is null)
                    continue;
                var linkNode = linkLookup[note];
                var nextLink = linkLookup[note.NextLink];
                linkNode.NextLink = nextLink;
                nextLink.PrevLink = linkNode;
            }
            foreach (var note in model.BackgroundNotes) {
                var data = note.GetUnderlyingData();
                if (data.NextLink is null)
                    continue;
                var linkNode = linkLookup[data];
                var nextLink = linkLookup[data.NextLink];
                linkNode.NextLink = nextLink;
                nextLink.PrevLink = linkNode;
            }
            foreach (var note in model.WarningNotes) {
                var data = note.GetUnderlyingData();
                if (data.NextLink is null)
                    continue;
                var linkNode = linkLookup[data];
                var nextLink = linkLookup[data.NextLink];
                linkNode.NextLink = nextLink;
                nextLink.PrevLink = linkNode;
            }

            var nodes = new List<IGameStageNoteNode>();
            foreach (var note in notes) {
                nodes.Add(note);
                if (note.Tail is { } tail) {
                    nodes.Add(tail);
                }
            }
            nodes.Sort(ModelComparers.ViaTimeUnique);

            Notes = notes;
            NoteNodes = nodes;
            BackgroundNotes = backgrounds;
            WarningNotes = warnings;
        }
    }
}
