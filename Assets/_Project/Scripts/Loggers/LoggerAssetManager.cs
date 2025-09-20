using Microsoft.Extensions.Logging;
using VTBeat.Asset;
using VTBeat.Diagnostic;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerAssetManager {
        public const string CATEGORY_NAME = nameof(AssetManager);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}