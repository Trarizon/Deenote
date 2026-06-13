#nullable enable

using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.CoreB.Logging
{
    public sealed class DebugLogger
    {
        public static DebugLogger Logger { get; } = new();

        private DebugLogger() { }

        public void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional("DEBUG")]
        public void LogDebug(object message)
        {
            UnityEngine.Debug.Log(message);
        }
    }
}
