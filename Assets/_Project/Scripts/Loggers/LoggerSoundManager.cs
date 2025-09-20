using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.Sound;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerSoundManager {
        public const string CATEGORY_NAME = nameof(SoundManager);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}