#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;

namespace Deenote.Editing.NotePlacement
{
    partial class NotePlacementContext : INotifyPropertyChanged<NotePlacementContext>
    {
        internal NotePrototypeModel MetaPrototype { get; }
        internal List<NotePrototypeModel> CurrentPrototypes { get; } = new();

        private NoteCoord _anchorCoord_bf;
        internal NoteCoord AnchorCoord
        {
            get => _anchorCoord_bf;
            set {
                if (_anchorCoord_bf != value) {
                    _anchorCoord_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(AnchorCoord)));
                }
            }
        }

        public NoteData ClonePrototypeData() { return MetaPrototype.ToDataNonLinkInfo(); }

        public void InsertPrototype(int index, out NotePrototypeModel model)
        {
            model = new NotePrototypeModel();
            CurrentPrototypes.Insert(index, model);
            PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CurrentPrototypes)));
        }

        public void AddPrototype(out NotePrototypeModel model)
        {
            model = new NotePrototypeModel();
            CurrentPrototypes.Add(model);
            PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CurrentPrototypes)));
        }

        public void RemovePrototypes(Range range)
        {
            CurrentPrototypes.RemoveRange(range);
            PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CurrentPrototypes)));
        }

        public void ReplacePrototypes(ReadOnlySpan<NotePrototypeModel> models)
        {
            CurrentPrototypes.Replace(models);
            PropertyChanged?.Invoke(this, new(nameof(CurrentPrototypes)));
        }
    }
}
