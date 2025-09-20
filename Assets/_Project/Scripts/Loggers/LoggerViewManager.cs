using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.View;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerViewManager {
        public const string CATEGORY_NAME = nameof(ViewManager);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}