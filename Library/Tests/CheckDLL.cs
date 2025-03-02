using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Tests
{
    internal class CheckDLL
    {
        internal CheckDLL()
        {
            PluginLoader plugins = PluginLoader.LoadPlugins([]);
            foreach (string plugin in plugins.Plugins.Keys)
            {
                Console.WriteLine(plugin + " is loaded.");
            }

            Dictionary<string, string> plugindllsha256 = [];
            foreach (string pfp in PluginLoader.PluginFilePaths.Keys)
            {
                string text = Encryption.FileSha256(PluginLoader.PluginFilePaths[pfp]);
                plugindllsha256.Add(pfp, text);
                Console.WriteLine(pfp + $" is {text}.");
            }

            LoginEventArgs e = new();
            plugins.OnBeforeLoginEvent(plugins, e);
            plugins.OnAfterLoginEvent(plugins, e);

            List<Character> list = [];

            GameModuleLoader modules = GameModuleLoader.LoadGameModules(FunGameInfo.FunGame.FunGame_Desktop, []);
            foreach (CharacterModule cm in modules.Characters.Values)
            {
                foreach (Character c in cm.Characters.Values)
                {
                    Console.WriteLine(c.Name);
                    list.Add(c);
                }
            }

            Dictionary<string, string> moduledllsha256 = [];
            foreach (string mfp in GameModuleLoader.ModuleFilePaths.Keys)
            {
                string text = Encryption.FileSha256(GameModuleLoader.ModuleFilePaths[mfp]);
                moduledllsha256.Add(mfp, text);
                Console.WriteLine(mfp + $" is {text}.");
            }

            GameModuleLoader serverModels = GameModuleLoader.LoadGameModules(FunGameInfo.FunGame.FunGame_Server, []);

            foreach (string moduledll in serverModels.ModuleServers.Keys)
            {
                if (!serverModels.ModuleServers[moduledll].IsAnonymous)
                {
                    string server = Encryption.FileSha256(GameModuleLoader.ModuleFilePaths[moduledll]);
                    string client = moduledllsha256[moduledll];
                    if (server == client)
                    {
                        Console.WriteLine(moduledll + $" is checked pass.");
                    }
                }
            }
        }
    }
}
