#nullable enable

using Deenote.Entities.Models;

namespace Deenote.Core.GameStage.Notes
{
    internal interface IGameStageNoteController
    {
        IStageSelectableNode Model { get; }
    }
}