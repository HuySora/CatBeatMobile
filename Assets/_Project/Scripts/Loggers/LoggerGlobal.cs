using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerGlobal {
        public const string CATEGORY_NAME = "Global";
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}