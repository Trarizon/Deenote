#nullable enable

namespace Deenote.Api.Experimental
{
    public interface IDeenoteContext 
    {
        IOperationHistory OperationHistory { get; }
        IChartEditor ChartEditor { get; }
        IGridsManager GridsManager { get; }

        IUIManager UI { get; }
    }
}
