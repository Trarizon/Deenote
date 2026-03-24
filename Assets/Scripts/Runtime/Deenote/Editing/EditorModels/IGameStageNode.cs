#nullable enable

namespace Deenote.Editing.EditorModels
{
    internal interface IGameStageTimeNode
    {
        float Time { get; }
    }

    internal interface IGameStageNode : IGameStageTimeNode
    {
        float Speed { get; }
        float Position { get; }
        bool IsComboNode { get; }
    }
}
