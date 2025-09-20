using Microsoft.Extensions.Logging;
using VTBeat.Diagnostic;
using VTBeat.Stage.MainMenu;

namespace VTBeat {
    // TODO: Source gen
    public static class LoggerMainMenuStage {
        public const string CATEGORY_NAME = nameof(MainMenuStage);
        public static readonly ILogger Log = LoggerManager.Factory.CreateLogger(CATEGORY_NAME);
    }
}