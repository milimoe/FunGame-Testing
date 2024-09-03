using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Testing.Solutions;

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
    foreach (Character c in cm.Characters)
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

if (list.Count > 3)
{
    Console.WriteLine();
    Console.WriteLine("Start!!!");
    Console.WriteLine();

    Character character1 = list[0].Copy();
    Character character2 = list[1].Copy();
    Character character3 = list[2].Copy();
    Character character4 = list[3].Copy();

    ActionQueue actionQueue = new();
    List<Character> characters = [character1, character2, character3, character4];

    // 初始顺序表排序
    actionQueue.CalculateInitialOrder(characters);
    Console.WriteLine();

    // 显示初始顺序表
    actionQueue.DisplayQueue();
    Console.WriteLine();

    // 模拟时间流逝
    int i = 1;
    while (i < 10)
    {
        // 检查是否有角色可以行动
        Character? characterToAct = actionQueue.NextCharacter();
        if (characterToAct != null)
        {
            Console.WriteLine($"=== Round {i++} ===");
            actionQueue.ProcessTurn(characterToAct);
            actionQueue.DisplayQueue();
            Console.WriteLine();
        }

        //Thread.Sleep(1); // 模拟时间流逝
        actionQueue.ReduceHardnessTimes();
    }

    Console.WriteLine("--- End ---");
}

Console.ReadKey();
