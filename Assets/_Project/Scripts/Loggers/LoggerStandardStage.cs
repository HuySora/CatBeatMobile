using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.Stage.Standard;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerStandardStage {
        public const string CATEGORY_NAME = nameof(StandardStage);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}