#nullable enable

using Deenote.Audio;
using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.Core.Project;
using Deenote.Library.Components;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote
{
    public sealed partial class ServiceProvider
    {
        public static StageDragSelector StageDragSelector { get; }
    }
}
