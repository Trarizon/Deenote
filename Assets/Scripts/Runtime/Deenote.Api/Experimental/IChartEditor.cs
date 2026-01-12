#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Api.Experimental
{
    public interface IChartEditor
    {
        void CutSelectedNotes();
        void CopySelectedNotes();
        void PasteNotes();

        void EditSelectedNotesCoord(Func<NoteCoord, NoteCoord> translate);
        void EditSelectedNotesPosition(Func<float, float> translate);
    }
}
