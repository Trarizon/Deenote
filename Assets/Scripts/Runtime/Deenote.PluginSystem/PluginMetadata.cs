#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Deenote.PluginSystem
{
    public sealed class PluginMetadata
    {
        public string EntryDll { get; }

        public static bool TryLoad(string filePath, [MaybeNullWhen(false)] out PluginMetadata metadata)
        {
            metadata = default;
            return false;
        }
    }
}
