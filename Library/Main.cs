using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.Core.Controllers;
using Oshima.Core.Utils;
using Oshima.FunGame.OshimaModules;
using Oshima.FunGame.OshimaModules.Skills;

CharacterModule cm = new();
cm.Load();
SkillModule sm = new();
sm.Load();
ItemModule im = new();
im.Load();

FunGameService.InitFunGame();
FunGameSimulation.InitFunGame();

FunGameController fc = new(new Logger<FunGameController>(new LoggerFactory()));
Console.WriteLine(string.Join("\r\n", fc.GetInventoryInfo2(3305106902, 23)));

//List<string> strings = FunGameSimulation.StartGame(false, false, true);
//strings.ForEach(Console.WriteLine);

User user = Factory.GetUser(1, FunGameService.GenerateRandomChineseUserName());
Console.WriteLine("你的名字是" + user.Username);

user.Inventory.Credits = 204824.59;
user.Inventory.Materials = 132415.2;
PluginConfig pc = new("saved", user.Id.ToString());
// 读取存档
pc.LoadConfig();

if (pc.Count == 0)
{
    Character exampleCharacter = FunGameService.Characters[1].Copy();
    exampleCharacter.Level = 30;
    Skill xlzh = new 心灵之火
    {
        Character = exampleCharacter,
        Level = 1
    };
    exampleCharacter.Skills.Add(xlzh);
    Skill tczl = new 天赐之力
    {
        Character = exampleCharacter,
        Level = 3
    };
    exampleCharacter.Skills.Add(tczl);

    List<Item> 魔法卡 = FunGameService.GenerateMagicCards(3, QualityType.Orange);
    Item? 魔法卡包 = FunGameService.ConflateMagicCardPack(魔法卡);
    if (魔法卡包 != null)
    {
        user.Inventory.Items.Add(魔法卡包);
        exampleCharacter.Equip(魔法卡包);
        Console.WriteLine(魔法卡包.ToString(false, true));
    }

    Item[] 饰品 = FunGameService.Equipment.Where(i => i.Id.ToString().StartsWith("14")).ToArray();
    Item sp = 饰品[Random.Shared.Next(饰品.Length)].Copy();
    sp.IsSellable = false;
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = 饰品[Random.Shared.Next(饰品.Length)].Copy();
    sp.IsTradable = false;
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = 饰品[Random.Shared.Next(饰品.Length)].Copy();
    sp.IsSellable = false;
    sp.IsTradable = false;
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = 饰品[Random.Shared.Next(饰品.Length)].Copy();
    sp.IsTradable = false;
    sp.NextTradableTime = DateTimeUtility.GetTradableTime();
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = 饰品[Random.Shared.Next(饰品.Length)].Copy();
    user.Inventory.Items.Add(sp);
    sp = 饰品[Random.Shared.Next(饰品.Length)].Copy();
    user.Inventory.Items.Add(sp);

    Console.WriteLine(exampleCharacter.GetInfo());

    user.Inventory.Characters.Add(exampleCharacter);
    Item mfk = FunGameService.GenerateMagicCard();
    user.Inventory.Items.Add(mfk);
    mfk = FunGameService.GenerateMagicCard();
    user.Inventory.Items.Add(mfk);
    mfk = FunGameService.GenerateMagicCard();
    user.Inventory.Items.Add(mfk);
    pc.Add("user", user);
}
else
{
    user = FunGameService.GetUser(pc);

    Console.WriteLine(user.Inventory.ToString(false));
    Console.WriteLine(user.Inventory.ToString(true));
}

// 保存存档
pc.SaveConfig();
//Item[] 武器 = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("11")).ToArray();
//Item[] 防具 = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("12")).ToArray();
//Item[] 鞋子 = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("13")).ToArray();
//Item[] 饰品 = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("14")).ToArray();
//Item? a = null, b = null, c = null, d = null;
//if (武器.Length > 0)
//{
//    a = 武器[Random.Shared.Next(武器.Length)];
//    exampleCharacter.Equip(a.Copy());
//}
//if (防具.Length > 0)
//{
//    b = 防具[Random.Shared.Next(防具.Length)];
//    exampleCharacter.Equip(b.Copy());
//}
//if (鞋子.Length > 0)
//{
//    c = 鞋子[Random.Shared.Next(鞋子.Length)];
//    exampleCharacter.Equip(c.Copy());
//}
//if (饰品.Length > 0)
//{
//    d = 饰品[Random.Shared.Next(饰品.Length)];
//    exampleCharacter.Equip(d.Copy());
//    d = 饰品[Random.Shared.Next(饰品.Length)];
//    exampleCharacter.Equip(d.Copy());
//}

//foreach (Skill s in FunGameSimulation.Magics)
//{
//    Skill s2 = s.Copy();
//    s2.Character = c;
//    Console.WriteLine(s2);
//    s2.Level++;
//    Console.WriteLine(s2);
//    c.Level = 60;
//    s2.Level = 8;
//    Console.WriteLine(s2);
//}

//Stopwatch stopwatch = new();
//stopwatch.Start();

//for (int i = 0; i < 2000; i++)
//{
//    Console.WriteLine($"{i}/2000");
//    FunGameSimulation.StartGame(false, false, false);
//    FunGameSimulation.StartGame(false, false, true);
//}

//stopwatch.Stop();
//Console.WriteLine($"执行时间：{stopwatch.Elapsed.TotalSeconds} 秒");

//IEnumerable<Character> cs = FunGameSimulation.CharacterStatistics.OrderByDescending(d => d.Value.Rating).ThenByDescending(d => d.Value.Winrates).Select(d => d.Key);
//Console.WriteLine("=== 个人模式排行榜 ===");
//foreach (Character c in cs)
//{
//    CharacterStatistics stats = FunGameSimulation.CharacterStatistics[c];
//    StringBuilder builder = new();

//    builder.AppendLine(c.ToString());
//    builder.AppendLine($"场次：{stats.Plays}");
//    builder.AppendLine($"胜率：{stats.Winrates * 100:0.##}%");
//    builder.AppendLine($"技术得分：{stats.Rating:0.0#}");

//    Console.WriteLine(builder.ToString());
//}

//IEnumerable<Character> cs2 = FunGameSimulation.TeamCharacterStatistics.OrderByDescending(d => d.Value.Rating).ThenByDescending(d => d.Value.Winrates).Select(d => d.Key);
//Console.WriteLine("=== 团队模式排行榜 ===");
//foreach (Character c in cs2)
//{
//    CharacterStatistics stats = FunGameSimulation.TeamCharacterStatistics[c];
//    StringBuilder builder = new();

//    builder.AppendLine(c.ToString());
//    builder.AppendLine($"场次：{stats.Plays}");
//    builder.AppendLine($"胜率：{stats.Winrates * 100:0.##}%");
//    builder.AppendLine($"技术得分：{stats.Rating:0.0#}");

//    Console.WriteLine(builder.ToString());
//}

string input = Console.ReadLine() ?? "";
while (true)
{
    if (input == "quit") break;

    if (input.StartsWith("sj"))
    {
        input = input.Replace("sj", "").Trim();
        if (int.TryParse(input, out int id))
        {
            Character character = FunGameService.Characters[Convert.ToInt32(id) - 1];
            if (FunGameSimulation.TeamCharacterStatistics.TryGetValue(character, out CharacterStatistics? stats) && stats != null)
            {
                StringBuilder builder = new();

                builder.AppendLine(character.ToString());
                builder.AppendLine($"总计造成伤害：{stats.TotalDamage:0.##} / 场均：{stats.AvgDamage:0.##}");
                builder.AppendLine($"总计造成物理伤害：{stats.TotalPhysicalDamage:0.##} / 场均：{stats.AvgPhysicalDamage:0.##}");
                builder.AppendLine($"总计造成魔法伤害：{stats.TotalMagicDamage:0.##} / 场均：{stats.AvgMagicDamage:0.##}");
                builder.AppendLine($"总计造成真实伤害：{stats.TotalRealDamage:0.##} / 场均：{stats.AvgRealDamage:0.##}");
                builder.AppendLine($"总计承受伤害：{stats.TotalTakenDamage:0.##} / 场均：{stats.AvgTakenDamage:0.##}");
                builder.AppendLine($"总计承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 场均：{stats.AvgTakenPhysicalDamage:0.##}");
                builder.AppendLine($"总计承受魔法伤害：{stats.TotalTakenMagicDamage:0.##} / 场均：{stats.AvgTakenMagicDamage:0.##}");
                builder.AppendLine($"总计承受真实伤害：{stats.TotalTakenRealDamage:0.##} / 场均：{stats.AvgTakenRealDamage:0.##}");
                builder.AppendLine($"总计存活回合数：{stats.LiveRound} / 场均：{stats.AvgLiveRound}");
                builder.AppendLine($"总计行动回合数：{stats.ActionTurn} / 场均：{stats.AvgActionTurn}");
                builder.AppendLine($"总计存活时长：{stats.LiveTime:0.##} / 场均：{stats.AvgLiveTime:0.##}");
                builder.AppendLine($"总计赚取金钱：{stats.TotalEarnedMoney} / 场均：{stats.AvgEarnedMoney}");
                builder.AppendLine($"每回合伤害：{stats.DamagePerRound:0.##}");
                builder.AppendLine($"每行动回合伤害：{stats.DamagePerTurn:0.##}");
                builder.AppendLine($"每秒伤害：{stats.DamagePerSecond:0.##}");
                builder.AppendLine($"总计击杀数：{stats.Kills}" + (stats.Plays != 0 ? $" / 场均：{(double)stats.Kills / stats.Plays:0.##}" : ""));
                builder.AppendLine($"总计死亡数：{stats.Deaths}" + (stats.Plays != 0 ? $" / 场均：{(double)stats.Deaths / stats.Plays:0.##}" : ""));
                builder.AppendLine($"总计助攻数：{stats.Assists}" + (stats.Plays != 0 ? $" / 场均：{(double)stats.Assists / stats.Plays:0.##}" : ""));
                builder.AppendLine($"总计首杀数：{stats.FirstKills}" + (stats.Plays != 0 ? $" / 首杀率：{(double)stats.FirstKills / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"总计首死数：{stats.FirstDeaths}" + (stats.Plays != 0 ? $" / 首死率：{(double)stats.FirstDeaths / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"总计参赛数：{stats.Plays}");
                builder.AppendLine($"总计冠军数：{stats.Wins}");
                builder.AppendLine($"总计前三数：{stats.Top3s}");
                builder.AppendLine($"总计败场数：{stats.Loses}");

                List<string> names = [.. FunGameSimulation.TeamCharacterStatistics.OrderByDescending(kv => kv.Value.Winrates).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"胜率：{stats.Winrates * 100:0.##}%（#{names.IndexOf(character.GetName()) + 1}）");

                names = [.. FunGameSimulation.TeamCharacterStatistics.OrderByDescending(kv => kv.Value.Rating).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"技术得分：{stats.Rating:0.##}（#{names.IndexOf(character.GetName()) + 1}）");

                Console.WriteLine(builder.ToString());
            }
        }
    }

    if (input.StartsWith("ss"))
    {
        input = input.Replace("ss", "").Trim();
        if (int.TryParse(input, out int id))
        {
            Character character = FunGameService.Characters[Convert.ToInt32(id) - 1];
            if (FunGameSimulation.CharacterStatistics.TryGetValue(character, out CharacterStatistics? stats) && stats != null)
            {
                StringBuilder builder = new();

                builder.AppendLine(character.ToString());
                builder.AppendLine($"总计造成伤害：{stats.TotalDamage:0.##} / 场均：{stats.AvgDamage:0.##}");
                builder.AppendLine($"总计造成物理伤害：{stats.TotalPhysicalDamage:0.##} / 场均：{stats.AvgPhysicalDamage:0.##}");
                builder.AppendLine($"总计造成魔法伤害：{stats.TotalMagicDamage:0.##} / 场均：{stats.AvgMagicDamage:0.##}");
                builder.AppendLine($"总计造成真实伤害：{stats.TotalRealDamage:0.##} / 场均：{stats.AvgRealDamage:0.##}");
                builder.AppendLine($"总计承受伤害：{stats.TotalTakenDamage:0.##} / 场均：{stats.AvgTakenDamage:0.##}");
                builder.AppendLine($"总计承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 场均：{stats.AvgTakenPhysicalDamage:0.##}");
                builder.AppendLine($"总计承受魔法伤害：{stats.TotalTakenMagicDamage:0.##} / 场均：{stats.AvgTakenMagicDamage:0.##}");
                builder.AppendLine($"总计承受真实伤害：{stats.TotalTakenRealDamage:0.##} / 场均：{stats.AvgTakenRealDamage:0.##}");
                builder.AppendLine($"总计存活回合数：{stats.LiveRound} / 场均：{stats.AvgLiveRound}");
                builder.AppendLine($"总计行动回合数：{stats.ActionTurn} / 场均：{stats.AvgActionTurn}");
                builder.AppendLine($"总计存活时长：{stats.LiveTime:0.##} / 场均：{stats.AvgLiveTime:0.##}");
                builder.AppendLine($"总计赚取金钱：{stats.TotalEarnedMoney} / 场均：{stats.AvgEarnedMoney}");
                builder.AppendLine($"每回合伤害：{stats.DamagePerRound:0.##}");
                builder.AppendLine($"每行动回合伤害：{stats.DamagePerTurn:0.##}");
                builder.AppendLine($"每秒伤害：{stats.DamagePerSecond:0.##}");
                builder.AppendLine($"总计击杀数：{stats.Kills}" + (stats.Plays != 0 ? $" / 场均：{(double)stats.Kills / stats.Plays:0.##}" : ""));
                builder.AppendLine($"总计死亡数：{stats.Deaths}" + (stats.Plays != 0 ? $" / 场均：{(double)stats.Deaths / stats.Plays:0.##}" : ""));
                builder.AppendLine($"总计助攻数：{stats.Assists}" + (stats.Plays != 0 ? $" / 场均：{(double)stats.Assists / stats.Plays:0.##}" : ""));
                builder.AppendLine($"总计首杀数：{stats.FirstKills}" + (stats.Plays != 0 ? $" / 首杀率：{(double)stats.FirstKills / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"总计首死数：{stats.FirstDeaths}" + (stats.Plays != 0 ? $" / 首死率：{(double)stats.FirstDeaths / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"总计参赛数：{stats.Plays}");
                builder.AppendLine($"总计冠军数：{stats.Wins}");
                builder.AppendLine($"总计前三数：{stats.Top3s}");
                builder.AppendLine($"总计败场数：{stats.Loses}");

                List<string> names = [.. FunGameSimulation.CharacterStatistics.OrderByDescending(kv => kv.Value.Winrates).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"胜率：{stats.Winrates * 100:0.##}%（#{names.IndexOf(character.GetName()) + 1}）");
                builder.AppendLine($"前三率：{stats.Top3rates * 100:0.##}%");

                names = [.. FunGameSimulation.CharacterStatistics.OrderByDescending(kv => kv.Value.Rating).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"技术得分：{stats.Rating:0.##}（#{names.IndexOf(character.GetName()) + 1}）");
                builder.AppendLine($"上次排名：{stats.LastRank} / 场均名次：{stats.AvgRank}");

                Console.WriteLine(builder.ToString());
            }
        }
    }
    input = Console.ReadLine() ?? "";
}
