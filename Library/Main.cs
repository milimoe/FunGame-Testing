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

// M = 0, W = 7, P1 = 1, P3 = 1
// M = 1, W = 6, P1 = 2, P3 = 0
// M = 2, W = 4, P1 = 0, P3 = 2
// M = 2, W = 5, P1 = 0, P3 = 0
// M = 3, W = 3, P1 = 1, P3 = 1
// M = 4, W = 2, P1 = 2, P3 = 0
// M = 5, W = 0, P1 = 0, P3 = 2
// M = 5, W = 1, P1 = 0, P3 = 0

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

    int clevel = 60;
    int slevel = 6;
    int mlevel = 8;

    // 升级和赋能
    for (int index = 0; index < characters.Count; index++)
    {
        Character c = characters[index];
        c.Level = clevel;
        c.NormalAttack.Level = mlevel;

        Skill 冰霜攻击 = new 冰霜攻击(c)
        {
            Level = mlevel
        };
        c.Skills.Add(冰霜攻击);

        if (c == character1)
        {
            Skill META马 = new META马(c)
            {
                Level = 1
            };
            c.Skills.Add(META马);

            Skill 力量爆发 = new 力量爆发(c)
            {
                Level = mlevel
            };
            c.Skills.Add(力量爆发);
        }

        if (c == character2)
        {
            Skill 心灵之火 = new 心灵之火(c)
            {
                Level = 1
            };
            c.Skills.Add(心灵之火);

            Skill 天赐之力 = new 天赐之力(c)
            {
                Level = slevel
            };
            c.Skills.Add(天赐之力);
        }
        
        if (c== character3)
        {
            Skill 魔法震荡 = new 魔法震荡(c)
            {
                Level = 1
            };
            c.Skills.Add(魔法震荡);

            Skill 魔法涌流 = new 魔法涌流(c)
            {
                Level = slevel
            };
            c.Skills.Add(魔法涌流);
        }

        if (c == character9)
        {
            Skill 疾风步 = new 疾风步(c)
            {
                Level = slevel
            };
            c.Skills.Add(疾风步);
        }

        if (c != character1 && c != character2 && c != character3)
        {
            Skill 天赐之力 = new 天赐之力(c)
            {
                Level = slevel
            };
            c.Skills.Add(天赐之力);
        }
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

        // 处理回合
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
