using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.Event;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerEventManager {
        public const string CATEGORY_NAME = nameof(EventManager);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}