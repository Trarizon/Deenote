#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library;
using System;
using System.Collections.Generic;
using Trarizon.Library.Linq;
using UnityEngine.Pool;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class ChartEditorModel : INotifyPropertyChanged<ChartEditorModel>
    {
        private string _name_bf = "";
        public string Name
        {
            get => _name_bf;
            set {
                if (Utils.SetField(ref _name_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Name)));
                }
            }
        }

        private Difficulty _difficulty_bf;
        public Difficulty Difficulty
        {
            get => _difficulty_bf;
            set {
                if (Utils.SetField(ref _difficulty_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Difficulty)));
                }
            }
        }

        private string _level_bf = "";
        public string Level
        {
            get => _level_bf;
            set {
                if (Utils.SetField(ref _level_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Level)));
                }
            }
        }

        private float _speed_bf;
        public float Speed
        {
            get => _speed_bf;
            set {
                if (Utils.SetField(ref _speed_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Speed)));
                }
            }
        }

        private int _remapMinVolume_bf;
        public int RemapMinVolume
        {
            get => _remapMinVolume_bf;
            set {
                if (Utils.SetField(ref _remapMinVolume_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(RemapMinVolume)));
                }
            }
        }

        private int _remapMaxVolume_bf;
        public int RemapMaxVolume
        {
            get => _remapMaxVolume_bf;
            set {
                if (Utils.SetField(ref _remapMaxVolume_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(RemapMaxVolume)));
                }
            }
        }

        public event Action<ChartEditorModel, PropertyEventArgs>? PropertyChanged;

        public ChartEditorModel(ChartModel model)
        {
            Name = model.Name;
            Difficulty = model.Difficulty;
            Level = model.Level;
            Speed = model.Speed;
            RemapMinVolume = model.RemapMinVolume;
            RemapMaxVolume = model.RemapMaxVolume;
            CloneNotes(model);

            NoteCollisionHelpers.InitializeCollision(this);
        }

        public ChartEditorModel()
        {
            Difficulty = Difficulty.Hard;
            Speed = 6;
            RemapMinVolume = 10;
            RemapMaxVolume = 70;
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
    }
}
