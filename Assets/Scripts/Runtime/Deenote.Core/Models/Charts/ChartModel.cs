#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;

namespace Deenote.CoreB.Models.Charts
{
    public sealed partial class ChartModel
    {
        private readonly ChartData _data;

        public string Name { get; set; } = "";
        public Difficulty Difficulty { get; set; }
        public string Level { get; set; } = "";

        /// <summary>
        /// Json speed property
        /// </summary>
        public float Speed { get => _data.Speed; set => _data.Speed = value; }
        public int RemapMinVolume { get => _data.RemapMinVolume; set => _data.RemapMinVolume = value; }
        public int RemapMaxVolume { get => _data.RemapMaxVolume; set => _data.RemapMaxVolume = value; }

        /// <summary>
        /// Notes visible on stage
        /// </summary>
        public List<NoteData> Notes { get; } = new();
        /// <summary>
        /// Background notes
        /// </summary>
        public List<BackgroundNoteModel> BackgroundNotes { get; } = new();
        /// <summary>
        /// Speed change warnings
        /// </summary>
        public List<WarningNoteModel> WarningNotes { get; } = new();
        [Obsolete("Editor current doesnt use lines, why should i serialize it in .dnt file?")]
        public List<SpeedLineData> SpeedLines { get; } = new();

        public ChartModel(float speed = 6f, int remapMinVolume = 10, int remapMaxVolume = 70)
        {
            _data = new ChartData(speed, remapMinVolume, remapMaxVolume);
        }

        public ChartModel(ChartData data)
        {
            _data = data;

            var notes = Marshal.SplitNotes(data.Notes.AsSpan());
            Notes = notes.VisibleNotes;
            BackgroundNotes = notes.BackgroundNotes;
            WarningNotes = notes.WarningNotes;
        }
    }
}
