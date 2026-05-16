#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;

namespace Deenote.Editing.NotePlacement
{
    public sealed class NotePlacementContext
    {
        internal NotePrototypeModel MetaPrototype { get; }
        internal List<NotePrototypeModel> CurrentPrototypes { get; } = new();
        
        internal NoteCoord AnchorCoord { get; set; }

        public NotePlacementContext()
        {
            MetaPrototype = new NotePrototypeModel {
                Speed = 1,
                Size = 1,
            };
        }

        public NoteData ClonePrototypeData() { return MetaPrototype.ToDataNonLinkInfo(); }

        public void InsertPrototype(int index, out NotePrototypeModel model)
        {
            model = new NotePrototypeModel();
            CurrentPrototypes.Insert(index, model);
        }

        public void AddPrototype(out NotePrototypeModel model)
        {
            model = new NotePrototypeModel();
            CurrentPrototypes.Add(model);
        }

        public void RemovePrototypes(Range range)
        {
            CurrentPrototypes.RemoveRange(range);
        }

        public void ReplacePrototypes(ReadOnlySpan<NotePrototypeModel> models)
        {
            CurrentPrototypes.Replace(models);
        }
    }
}
