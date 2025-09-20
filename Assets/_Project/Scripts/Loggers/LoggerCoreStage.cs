using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.Stage.Core;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerCoreStage {
        public const string CATEGORY_NAME = nameof(CoreStage);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}