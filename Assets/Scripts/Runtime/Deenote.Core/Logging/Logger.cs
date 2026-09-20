using UnityEngine;

namespace Deenote.Core.Logging
{
    public static class Logger
    {
        public static void LogTrace(string message)
            => Debug.Log(message);

        public static void LogDebug(string message)
            => Debug.Log(message);

        public static void LogError(string message)
            => Debug.LogError(message);
    }
}