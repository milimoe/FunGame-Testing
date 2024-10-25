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

            Dictionary<string, string> plugindllsha512 = [];
            foreach (string pfp in PluginLoader.PluginFilePaths.Keys)
            {
                string text = Encryption.FileSha512(PluginLoader.PluginFilePaths[pfp]);
                plugindllsha512.Add(pfp, text);
                Console.WriteLine(pfp + $" is {text}.");
            }

            LoginEventArgs e = new();
            plugins.OnBeforeLoginEvent(plugins, e);
            if (!e.Cancel)
            {
                plugins.OnSucceedLoginEvent(plugins, e);
                plugins.OnFailedLoginEvent(plugins, e);
            }
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

            Dictionary<string, string> moduledllsha512 = [];
            foreach (string mfp in GameModuleLoader.ModuleFilePaths.Keys)
            {
                string text = Encryption.FileSha512(GameModuleLoader.ModuleFilePaths[mfp]);
                moduledllsha512.Add(mfp, text);
                Console.WriteLine(mfp + $" is {text}.");
            }

            foreach (string moduledll in moduledllsha512.Keys)
            {
                string server = moduledllsha512[moduledll];
                if (plugindllsha512.TryGetValue(moduledll, out string? client) && client != "" && server == client)
                {
                    Console.WriteLine(moduledll + $" is checked pass.");
                }
            }
        }
    }
}
