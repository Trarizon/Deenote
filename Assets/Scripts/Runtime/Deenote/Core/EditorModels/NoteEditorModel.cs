#nullable enable

using System;
using System.ComponentModel;
using UnityEngine;

namespace Deenote.Core.EditorModels
{
    internal sealed class NoteEditorModel
    {
        public bool IsSelected { get; set; }
        public bool IsCollided { get; set; }
    }
}
