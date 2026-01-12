#nullable enable

namespace Deenote.Api.Experimental
{
    public interface IOperationHistory
    {
        bool Undo();
        bool Redo();
    }
}
