#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Api.Operations;
using Deenote.Editing.EditorModels;
using System;

namespace Deenote.Editing.Operations
{
    internal interface IChartOperation : IOperation
    {
        ChartEditorModel Chart { get; }
    }

    internal abstract class NotifiableChartOperation: NotifiableOperation, IChartOperation
    {
        public ChartEditorModel Chart { get; }
        protected NotifiableChartOperation(ChartEditorModel chart) 
            => Chart = chart;
    }

    internal abstract class NotifiableChartOperation<TArgs> : NotifiableOperation<TArgs>, IChartOperation
    {
        public ChartEditorModel Chart { get; }
        protected NotifiableChartOperation(ChartEditorModel chart) 
            => Chart = chart;
    }
}
