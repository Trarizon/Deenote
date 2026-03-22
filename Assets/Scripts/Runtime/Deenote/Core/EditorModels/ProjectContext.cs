#nullable enable

using Deenote.Entities.Models;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Deenote.Core.EditorModels
{
    internal sealed class ProjectContext
    {
        public ChartModel? CurrentChart { get; }
        public EditorContext EditorContext { get; set; }
    }
}
