using System.IO;

namespace HaveYouReadThis
{
    public static class HLog
    {
        private static bool debugLogging = false;

        public static void SetLogLevel()
        {
            debugLogging = File.Exists(Path.Combine(GameIO.GetGameDir(string.Empty), "Mods", "HaveYouReadThis",
                "DebugLoggingEnabled.txt"));
        }
        
        public static void Debug(string msg)
        {
            if (debugLogging)
                Log.Out(msg);
        }
        
    }
}