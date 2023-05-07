using System;
using System.Collections.Generic;
using System.Text;

namespace SORRY
{
    internal static class SORRYLog
    {
        internal static void Info(string message) => SORRYPlugin.Instance.ModLogger.LogInfo(message);
        internal static void Debug(string message) => SORRYPlugin.Instance.ModLogger.LogDebug(message);
        internal static void Warning(string message) => SORRYPlugin.Instance.ModLogger.LogWarning(message);
        internal static void Error(string message) => SORRYPlugin.Instance.ModLogger.LogError(message);
        internal static void Fatal(string message) => SORRYPlugin.Instance.ModLogger.LogFatal(message);
    }
}
