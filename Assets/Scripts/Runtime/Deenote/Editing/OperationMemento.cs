using Deenote.Entities.Operations;
using Deenote.Library.Collections.Generic;
using UnityEngine;

namespace Deenote.Editing
{
    public sealed class OperationMemento2
    {
        private const int MaxOperationCount = 100;

        private readonly Memento<IUndoableOperation> _memento;
        private int _saveOffset;

        public bool CanRedo => _memento.ActiveCount < _memento.Count;

        public bool CanUndo => _memento.ActiveCount > 0;

        public bool HasUnsavedChange => _saveOffset != 0;

        public OperationMemento2()
        {
            _memento = new(MaxOperationCount);
        }

        public void Do(IUndoableOperation? operation)
        {
            if (operation is null)
                return;

            _memento.Add(operation);
            operation.Redo();
            _saveOffset++;
        }

        public void Redo()
        {
            Debug.Log("Redo");
            _memento.Reapply(out var op);
            op.Redo();
            _saveOffset++;
        }

        public void Undo()
        {
            App.Logger.LogDebug("Undo");
            _memento.Rollback(out var op);
            op.Undo();
            _saveOffset--;
            Debug.Assert(_saveOffset >= 0);
        }

        public void Reset()
        {
            _memento.Clear();
            _saveOffset = 0;
        }

        public void SaveAtCurrent()
        {
            _saveOffset = 0;
        }
    }
}
