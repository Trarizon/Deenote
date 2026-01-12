#nullable enable

using System.Collections.Immutable;

namespace Deenote.Api.Experimental.Plugin
{
    public interface IDeenotePluginProvider
    {
        string? GetGroupName(string languageCode);
        ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; }
    }
}