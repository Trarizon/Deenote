#nullable enable

using Deenote.Entities.Models;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Deenote.Core.EditorModels
{
    internal sealed class ModelContext
    {
        public ChartModel? CurrentChart { get; }
    }
}
