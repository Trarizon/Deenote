#nullable enable

using Deenote.Api.Operations;

namespace Deenote.Editing.Operations.Components
{
    internal sealed class CombinedOperation : IOperation
    {
        private readonly IOperation _first;
        private readonly IOperation _second;

        public CombinedOperation(IOperation first, IOperation second)
        {
            _first = first;
            _second = second;
        }

        public void Redo()
        {
            _first.Redo();
            _second.Redo();
        }

        public void Undo()
        {
            _second.Undo();
            _first.Undo();
        }
    }
}
