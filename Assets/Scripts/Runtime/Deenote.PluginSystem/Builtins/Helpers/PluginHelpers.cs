#nullable enable

using System;

namespace Deenote.PluginSystem.Builtins.Helpers
{
    internal static class PluginHelpers
    {
        public static Func<string, string> Text(string fallback, params (string Language, string Text)[] texts)
        {
            return lang =>
            {
                foreach (var (k, v) in texts) {
                    if (lang == k)
                        return v;
                }
                return fallback;
            };
        }
    }
}
