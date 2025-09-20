using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.UnityObject;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerUObjectManager {
        public const string CATEGORY_NAME = nameof(UObjectManager);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}