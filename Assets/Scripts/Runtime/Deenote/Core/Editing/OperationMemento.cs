#nullable enable

using Deenote.Api.Operations;
using Deenote.Editing.EditorModels;
using Deenote.Library.Collections.Generic;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class OperationMemento
    {
        private const int MaxOperationUndoCount = 200;

        private readonly Memento<IOperation> _memento;
        private int _saveOffset;

        public bool CanRedo => _memento.ActiveCount >= _memento.Count;

        public bool CanUndo => _memento.ActiveCount > 0;

        public bool HasUnsavedChange => _saveOffset != 0;

        public OperationMemento()
        {
            _memento = new(MaxOperationUndoCount);
        }

        public void Do(IOperation? operation)
        {
            if (operation is null)
                return;

            _memento.Add(operation);
            operation.Redo();
            _saveOffset++;
        }

        /// <summary>
        /// If next operation is a <see cref="IUndoableChartOperation"/>, redo the operation only when 
        /// <c>Chart</c> is <paramref name="operationChart"/>
        /// <br/>
        /// otherwise, always redo the operation
        /// </summary>
        /// <param name="operationChart"></param>
        public void Redo(ChartEditorModel? operationChart)
        {
            if (_memento.TryPeekFirstInactive(out var operation)) {
                goto Apply;
            }
            return;

        Apply:
            Debug.Log("Redo");
            _memento.Reapply(out var op);
            //Debug.Assert(op == operation);
            operation.Redo();
            _saveOffset++;
        }

        /// <summary>
        /// If previous operation is a <see cref="IUndoableChartOperation"/>, undo the operation only when 
        /// <c>Chart</c> is <paramref name="operationChart"/>
        /// <br/>
        /// otherwise, always redo the operation
        /// </summary>
        /// <param name="operationChart"></param>
        public void Undo(ChartEditorModel? operationChart)
        {
            if (_memento.TryPeekLastActive(out var operation)) {
                goto Apply;
            }
            return;

        Apply:
            Debug.Log("Undo");
            _memento.Rollback(out var op);
            Debug.Assert(op == operation);
            operation.Undo();
            _saveOffset--;
        }

        public void Reset()
        {
            _memento.Clear();
            _saveOffset = 0;
        }

        public void MarkSaveAtCurrent()
        {
            _saveOffset = 0;
        }
    }
}