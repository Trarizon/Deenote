#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Library.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Linq;
using UnityEngine.Pool;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class ChartEditorModel : ObservableObject
    {
        [ObservableProperty] string _name = "";
        [ObservableProperty] Difficulty _difficulty;
        [ObservableProperty] string _level = "";

        [ObservableProperty] float _speed;
        [ObservableProperty] int _remapMinVolume;
        [ObservableProperty] int _remapMaxVolume;

        public List<NoteEditorModel> Notes { get; private set; }
        internal List<IGameStageNoteNode> NoteNodes { get; private set; }
        internal List<BackgroundNoteEditorModel> BackgroundNotes { get; private set; }
        internal List<WarningNoteEditorModel> WarningNotes { get; private set; }

        internal Dictionary<ICollidableNote, List<ICollidableNote>> Collisions { get; } = new();

        internal ChartEditorModel(ChartModel model)
        {
            _name = model.Name;
            _difficulty = model.Difficulty;
            _level = model.Level;
            _speed = model.Speed;
            _remapMinVolume = model.RemapMinVolume;
            _remapMaxVolume = model.RemapMaxVolume;
            CloneNotes(model);
        }

        public ChartEditorModel()
        {
            _difficulty = Difficulty.Hard;
            _speed = 6;
            _remapMinVolume = 10;
            _remapMaxVolume = 70;
            Notes = new List<NoteEditorModel>();
            NoteNodes = new List<IGameStageNoteNode>();
            BackgroundNotes = new List<BackgroundNoteEditorModel>();
            WarningNotes = new List<WarningNoteEditorModel>();
        }

        public ChartModel ToModel()
        {
            var chart = new ChartModel(Speed, RemapMinVolume, RemapMaxVolume) {
                Name = Name,
                Difficulty = Difficulty,
                Level = Level,
            };

            using var dp_linkLoopup = DictionaryPool<INoteLink, NoteData>.Get(out var linkLookup);

            foreach (var note in Notes) {
                var model = note.ToDataNonLinkInfo();
                linkLookup.Add(note, model);
                chart.Notes.Add(model);
            }
            foreach (var note in BackgroundNotes) {
                var model = note.ToModelNonLinkInfo();
                linkLookup.Add(note, model.GetUnderlyingData());
                chart.BackgroundNotes.Add(model);
            }
            foreach (var note in WarningNotes) {
                var model = note.ToModelNonLinkInfo();
                linkLookup.Add(note, model.GetUnderlyingData());
                chart.WarningNotes.Add(model);
            }

            foreach (var note in Notes) {
                if (note.NextLink is null)
                    continue;
                var link = linkLookup[note];
                var next = linkLookup[note.NextLink];
                NoteData.Marshal.Link(link, next);
            }
            foreach (var note in BackgroundNotes) {
                if (note.NextLink is null)
                    continue;
                var link = linkLookup[note];
                var next = linkLookup[note.NextLink];
                NoteData.Marshal.Link(link, next);
            }
            foreach (var note in WarningNotes) {
                if (note.NextLink is null)
                    continue;
                var link = linkLookup[note];
                var next = linkLookup[note.NextLink];
                NoteData.Marshal.Link(link, next);
            }

            return chart;
        }

        public ChartData ToData()
        {
            var chart = new ChartData(Speed, RemapMinVolume, RemapMaxVolume);

            using var dp_linkLoopup = DictionaryPool<INoteLink, NoteData>.Get(out var linkLookup);

            var notes = new List<NoteData>();
            var backgrounds = new List<NoteData>();
            var warnings = new List<NoteData>();

            foreach (var note in Notes) {
                var model = note.ToDataNonLinkInfo();
                linkLookup.Add(note, model);
                notes.Add(model);
            }
            foreach (var note in BackgroundNotes) {
                var model = note.ToModelNonLinkInfo().GetUnderlyingData();
                linkLookup.Add(note, model);
                backgrounds.Add(model);
            }
            foreach (var note in WarningNotes) {
                var model = note.ToModelNonLinkInfo().GetUnderlyingData();
                linkLookup.Add(note, model);
                warnings.Add(model);
            }

            foreach (var note in Notes) {
                if (note.NextLink is null)
                    continue;
                var link = linkLookup[note];
                var next = linkLookup[note.NextLink];
                NoteData.Marshal.Link(link, next);
            }
            foreach (var note in BackgroundNotes) {
                if (note.NextLink is null)
                    continue;
                var link = linkLookup[note];
                var next = linkLookup[note.NextLink];
                NoteData.Marshal.Link(link, next);
            }
            foreach (var note in WarningNotes) {
                if (note.NextLink is null)
                    continue;
                var link = linkLookup[note];
                var next = linkLookup[note.NextLink];
                NoteData.Marshal.Link(link, next);
            }

            chart.Notes.AddRange(notes
                .Merge(backgrounds, NoteComparers.ViaTime)
                .Merge(warnings, NoteComparers.ViaTime));

            chart.SpeedLines.AddRange(SpeedLineRangeData.Marshal.FromNotes(chart.Notes));

            return chart;
        }

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
