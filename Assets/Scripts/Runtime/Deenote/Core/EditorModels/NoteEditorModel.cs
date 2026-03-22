#nullable enable

using Deenote.Entities.Models;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Deenote.Core.EditorModels
{
    public sealed class NoteEditorModel
    {
        private uint _uid;
        private readonly NoteModel _model;

        public bool IsSelected { get; set; }
        public bool IsCollided { get; set; }

        public NoteEditorModel(NoteModel model)
        {
            _model = model;
        }
    }
}
