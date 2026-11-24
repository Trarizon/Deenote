using UnityEngine;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace Deenote.Core.Logging
{
    public class Logger
    {
        public void LogTrace(string message)
            => Debug.Log($"[{Time.frameCount}] [T] {message}");

        public void LogInfo(string message)
            => Debug.Log($"[{Time.frameCount}] [I] {message}");

        public void LogDebug(string message)
            => Debug.Log($"[{Time.frameCount}] [D] {message}");

        public void LogWarning(string message)
            => Debug.LogWarning($"[{Time.frameCount}] [W] {message}");

        public void LogError(string message)
            => Debug.LogError($"[{Time.frameCount}] [E] {message}");

        [Conditional("DEBUG")]
        public void DebugLogTrace(string message)
            => Debug.Log($"[{Time.frameCount}] [T] {message}");
    }
}