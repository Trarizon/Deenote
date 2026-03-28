#nullable enable

using Deenote.CoreB.Models.Notes;

namespace Deenote.Editing.EditorModels
{
    internal interface IGameStageNoteNode : INoteTimeUnique, INoteLocation
    {
        new float Time { get; }
        new float Speed { get; }
        new  float Position { get; }
        bool IsComboNode { get; }

        float INoteTime.Time => Time;
        float INoteSpeed.Speed => Speed;
        float INoteLocation.Position => Position;
    }
}
