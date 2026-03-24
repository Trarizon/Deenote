#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Models.Notes.Comparing;
using Deenote.Library.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Linq;
using UnityEngine.Pool;

namespace Deenote.Editing.EditorModels
{
    internal sealed partial class ChartEditorModel : ObservableObject
    {
        [ObservableProperty] string _name = "";
        [ObservableProperty] Difficulty _difficulty;
        [ObservableProperty] string _level = "";

        [ObservableProperty] float _speed;
        [ObservableProperty] int _remapMinVolume;
        [ObservableProperty] int _remapMaxVolume;

        public List<NoteEditorModel> Notes { get; private set; }
        public List<BackgroundNoteEditorModel> BackgroundNotes { get; private set; }
        public List<WarningNoteEditorModel> WarningNotes { get; private set; }

        public ChartEditorModel(ChartModel model)
        {
            _name = model.Name;
            _difficulty = model.Difficulty;
            _level = model.Level;
            _speed = model.Speed;
            _remapMinVolume = model.RemapMinVolume;
            _remapMaxVolume = model.RemapMaxVolume;
            CloneNotes(model);
        }

        public ChartModel ToModel()
        {
            var chart = new ChartModel(Speed, RemapMinVolume, RemapMaxVolume) {
                Name = Name,
                Difficulty = Difficulty,
                Level = Level,
            };

            using var dp_linkLoopup = DictionaryPool<INoteLinkNode, NoteData>.Get(out var linkLookup);

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

            using var dp_linkLoopup = DictionaryPool<INoteLinkNode, NoteData>.Get(out var linkLookup);

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

        [MemberNotNull(nameof(Notes), nameof(BackgroundNotes), nameof(WarningNotes))]
        private void CloneNotes(ChartModel model)
        {
            using var dp_linkLookup = DictionaryPool<NoteData, INoteLinkNode>.Get(out var linkLookup);

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

            Notes = notes;
            BackgroundNotes = backgrounds;
            WarningNotes = warnings;
        }
    }
}
