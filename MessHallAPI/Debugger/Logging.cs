using BepInEx;
using BepInEx.Logging;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Debugger
{
    public class Logging
    {
        static void Print(string message = "", int type = 0)
        {
                Logger.CreateLogSource(message);
        }

        public static void Log(string message)
        {
            Print($"[{ModName}]: {message}", 0);
        }

        public static void Warn(string message)
        {
            Print($"[{ModName}]: {message}", 1);
        }

        public static void Error(string message)
        {
            Print($"[{ModName}]: {message}", 2);
        }

        public static void DebugLog(string message)
        {
            if (DebugMode)
            {
                Print($"[{ModName}]: [DEBUG]: {message}", 0);
            }
        }

        public static void DebugWarn(string message)
        {
            if (DebugMode)
            {
                Print($"[{ModName}]: [DEBUG]: {message}", 1);
            }
        }

        public static void DebugError(string message)
        {
            if (DebugMode)
            {
                Print($"[{ModName}]: [DEBUG]: {message}", 2);
            }
        }
    }
}
