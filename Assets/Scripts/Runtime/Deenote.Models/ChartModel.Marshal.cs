using Deenote.Models.Comparisons;
using System;
using System.Collections.Generic;

namespace Deenote.Models
{
    partial class ChartModel
    {
        internal static class Marshal
        {
            public static SplitNotesResult SplitNotes(ReadOnlySpan<NoteData> notes)
            {
                if (notes.IsEmpty) {
                    return default;
                }

                var visibles = new List<NoteData>();
                var backgrounds = new List<BackgroundNoteModel>();
                var warnings = new List<WarningNoteModel>();

                foreach (var note in notes) {
                    if (note.IsVisibleOnStage()) {
                        visibles.Add(note);
                    }
                    else if (note.WarningType is WarningType.SpeedChange) {
                        warnings.Add(new WarningNoteModel(note));
                    }
                    else {
                        backgrounds.Add(new BackgroundNoteModel(note));
                    }
                }
                visibles.Sort(NoteComparers.ByTime);
                backgrounds.Sort(NoteComparers.ByTime);
                warnings.Sort(NoteComparers.ByTime);
                return new SplitNotesResult(visibles, backgrounds, warnings);
            }

            public readonly record struct SplitNotesResult(
                List<NoteData> VisibleNotes,
                List<BackgroundNoteModel> BackgroundNotes,
                List<WarningNoteModel> WarningNotes);
        }
    }
}