#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Deenote.Api.Experimental.Plugin
{
    public interface IDeenotePlugin
    {
        string GetName(string languageCode);
        string? GetDescription(string languageCode);
        Task ExecuteAsync(IDeenoteContext context, CancellationToken cancellationToken = default);
    }
}