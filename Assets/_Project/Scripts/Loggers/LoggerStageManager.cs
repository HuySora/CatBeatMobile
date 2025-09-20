using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.Stage;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerStageManager {
        public const string CATEGORY_NAME = nameof(StageManager);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}