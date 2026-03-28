#nullable enable

namespace Deenote.Api.Operations
{
    public interface IOperation
    {
        void Redo();
        void Undo();
    }
}
