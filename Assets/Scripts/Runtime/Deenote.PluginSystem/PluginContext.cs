#nullable enable

using Deenote.Api.Experimental;

namespace Deenote.PluginSystem
{
    public sealed class PluginContext : IDeenoteContext
    {
        public static PluginContext Instance { get; } = new();

        public IOperationHistory OperationHistory { get; }

        public IChartEditor ChartEditor { get; }

        public IGridsManager GridsManager { get; }

        public IUIManager UI { get; }
    }
}
