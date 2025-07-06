using System.IO;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using Path = System.IO.Path;

namespace PartyScreenEnhancements.Saving
{
    public static class Directories
    {
        public static void Initialize()
        {
            Directory.CreateDirectory(GetConfigPath());
        }

        public static string GetConfigPathForFile(string filename)
        {
            return Path.Combine(GetConfigPath(), filename);
        }

        public static string GetConfigPath()
        {
            // Credits to Discord user @Sidies from the Modding Discord.
            var propertyInfo = Common.PlatformFileHelper.GetType().GetProperty("DocumentsPath", BindingFlags.NonPublic
                | BindingFlags.Instance);
            var documentsFilePath = (string)propertyInfo.GetValue(Common.PlatformFileHelper);

            documentsFilePath = Path.Combine(
                documentsFilePath,
                TaleWorlds.Engine.Utilities.GetApplicationName(),
                EngineFilePaths.ConfigsPath.Path,
                "Mods"
            );

            return documentsFilePath;
        }
    }
}