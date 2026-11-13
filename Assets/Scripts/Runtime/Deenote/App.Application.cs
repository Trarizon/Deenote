using Deenote.Core;
using Deenote.Core.Logging;
using Deenote.CoreB;
using Deenote.Systems;

namespace Deenote
{
    partial class App
    {
        public static new Logger Logger => ((Application)Current).Logger;

        public SaveSystem SaveSystem { get; } = new();
        public ResolutionManager ResolutionManager { get; } = new();
        public ExceptionHandler ExceptionHandler { get; } = new();
        public VersionManager VersionManager { get; } = new();
        
        internal AppGlobalConfigs Configs { get; private set; }
    }
}
