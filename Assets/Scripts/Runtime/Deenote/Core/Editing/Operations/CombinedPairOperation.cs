#nullable enable

using Deenote.Api.Operations;

namespace Deenote.Core.Editing.Operations
{
    public sealed class CombinedPairOperation : IOperation
    {
        private readonly IOperation _first;
        private readonly IOperation _second;

        public CombinedPairOperation(IOperation first, IOperation second)
        {
            _first = first;
            _second = second;
        }

        void IOperation.Redo()
        {
            _first.Redo();
            _second.Redo();
        }

        void IOperation.Undo()
        {
            _second.Undo();
            _first.Undo();
        }
    }
}
