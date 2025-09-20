using Microsoft.Extensions.Logging;
using ZLogger.Unity;

namespace VTBeat.Diagnostic {
    public static class LoggerManager {
        public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            builder.SetMinimumLevel(LogLevel.Trace);
#else
            builder.SetMinimumLevel(LogLevel.Error);
#endif
            builder.AddFilter(LoggerAssetManager.CATEGORY_NAME, LogLevel.Information);
            // builder.AddFilter(LoggerCoreLevel.CATEGORY_NAME, LogLevel.None);
            // builder.AddFilter(LoggerEventManager.CATEGORY_NAME, LogLevel.None);
            // builder.AddFilter(LoggerGlobal.CATEGORY_NAME, LogLevel.None);
            // builder.AddFilter(LoggerLevelManager.CATEGORY_NAME, LogLevel.None);
            // builder.AddFilter(LoggerMainMenuLevel.CATEGORY_NAME, LogLevel.None);
            // builder.AddFilter(LoggerSoundManager.CATEGORY_NAME, LogLevel.None);
            builder.AddFilter(LoggerUObjectManager.CATEGORY_NAME, LogLevel.Information);
            // builder.AddFilter(LoggerViewManager.CATEGORY_NAME, LogLevel.None);
            builder.AddZLoggerUnityDebug();
        });
    }
}