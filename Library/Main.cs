using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Event;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Testing.Skills;

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
    Character character5 = list[4].Copy();
    Character character6 = list[5].Copy();
    Character character7 = list[6].Copy();
    Character character8 = list[7].Copy();
    Character character9 = list[8].Copy();
    Character character10 = list[9].Copy();
    Character character11 = list[10].Copy();
    Character character12 = list[11].Copy();

    List<Character> characters = [
        character1, character2, character3, character4,
        character5, character6, character7, character8,
        character9, character10, character11, character12
    ];

    // 升级和赋能
    for (int index = 0; index < characters.Count; index++)
    {
        Character c = characters[index];
        c.Level = 60;
        c.NormalAttack.Level += 7;

        Skill 冰霜攻击 = new 冰霜攻击(c);
        冰霜攻击.Level += 8;
        c.Skills.Add(冰霜攻击);

        if (c.ToString() == character1.ToString())
        {
            Skill 大岛特性 = new 大岛特性(c);
            大岛特性.Level++;
            c.Skills.Add(大岛特性);
        }

        Skill 天赐之力 = new 天赐之力(c);
        天赐之力.Level += 6;
        c.Skills.Add(天赐之力);
    }

    // 显示角色信息
    characters.ForEach(c => Console.WriteLine(c.GetInfo()));

    // 创建顺序表并排序
    ActionQueue actionQueue = new(characters, Console.WriteLine);
    Console.WriteLine();

    // 显示初始顺序表
    actionQueue.DisplayQueue();
    Console.WriteLine();

    // 总回合数
    int i = 1;
    while (i < 999)
    {
        // 检查是否有角色可以行动
        Character? characterToAct = actionQueue.NextCharacter();
        if (characterToAct != null)
        {
            Console.WriteLine($"=== Round {i++} ===");
            Console.WriteLine("现在是 [ " + characterToAct + " ] 的回合！");

            bool isGameEnd = actionQueue.ProcessTurn(characterToAct);
            if (isGameEnd)
            {
                break;
            }

            actionQueue.DisplayQueue();
            Console.WriteLine();
        }

        // 模拟时间流逝
        actionQueue.TimeLapse();
    }

    Console.WriteLine("--- End ---");
}

Console.ReadKey();
