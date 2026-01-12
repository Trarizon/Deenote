#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.Api.Experimental.Plugin
{
    public static class DeenotePlugin
    {
        public static IDeenotePlugin CreateSync(Func<string, string> getLocalizedName, Action<IDeenoteContext> execute)
            => new DelegateDeenotePlugin(getLocalizedName, null, (ctx, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled(ct);
                execute(ctx);
                return Task.CompletedTask;
            });

        public static IDeenotePlugin Create(Func<string, string> getLocalizedName, Func<IDeenoteContext, CancellationToken, Task> execute)
            => new DelegateDeenotePlugin(getLocalizedName, null, execute);
        public static IDeenotePlugin Create(Func<string, string> getLocalizedName, Func<string, string>? getLocalizedDescription, Func<IDeenoteContext, CancellationToken, Task> execute)
            => new DelegateDeenotePlugin(getLocalizedName, getLocalizedDescription, execute);
    }

    internal sealed class DelegateDeenotePlugin : IDeenotePlugin
    {
        private Func<string, string> _getLocalizedName;
        private Func<string, string>? _getLocalizedDescription;
        private Func<IDeenoteContext, CancellationToken, Task> _execute;

        public DelegateDeenotePlugin(Func<string, string> getLocalizedName, Func<string, string>? getLocalizedDescription, Func<IDeenoteContext, CancellationToken, Task> execute)
        {
            _getLocalizedName = getLocalizedName;
            _getLocalizedDescription = getLocalizedDescription;
            _execute = execute;
        }

        public Task ExecuteAsync(IDeenoteContext context, CancellationToken cancellationToken = default) => _execute(context, cancellationToken);
        public string? GetDescription(string languageCode) => _getLocalizedDescription?.Invoke(languageCode);
        public string GetName(string languageCode) => _getLocalizedName(languageCode);
    }
}
