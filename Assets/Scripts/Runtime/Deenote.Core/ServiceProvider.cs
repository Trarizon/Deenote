#nullable enable

using Deenote.CoreB.Logging;
using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.CoreB
{
    public static class Services
    {
        public static DebugLogger Logger => DebugLogger.Logger;
    }
}
