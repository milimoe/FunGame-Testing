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
Console.WriteLine("���������" + user.Username);

user.Inventory.Credits = 204824.59;
user.Inventory.Materials = 132415.2;
PluginConfig pc = new("saved", user.Id.ToString());
// ��ȡ�浵
pc.LoadConfig();

if (pc.Count == 0)
{
    Character exampleCharacter = FunGameService.Characters[1].Copy();
    exampleCharacter.Level = 30;
    Skill xlzh = new ����֮��
    {
        Character = exampleCharacter,
        Level = 1
    };
    exampleCharacter.Skills.Add(xlzh);
    Skill tczl = new ���֮��
    {
        Character = exampleCharacter,
        Level = 3
    };
    exampleCharacter.Skills.Add(tczl);

    List<Item> ħ���� = FunGameService.GenerateMagicCards(3, QualityType.Orange);
    Item? ħ������ = FunGameService.ConflateMagicCardPack(ħ����);
    if (ħ������ != null)
    {
        user.Inventory.Items.Add(ħ������);
        exampleCharacter.Equip(ħ������);
        Console.WriteLine(ħ������.ToString(false, true));
    }

    Item[] ��Ʒ = FunGameService.Equipment.Where(i => i.Id.ToString().StartsWith("14")).ToArray();
    Item sp = ��Ʒ[Random.Shared.Next(��Ʒ.Length)].Copy();
    sp.IsSellable = false;
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = ��Ʒ[Random.Shared.Next(��Ʒ.Length)].Copy();
    sp.IsTradable = false;
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = ��Ʒ[Random.Shared.Next(��Ʒ.Length)].Copy();
    sp.IsSellable = false;
    sp.IsTradable = false;
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = ��Ʒ[Random.Shared.Next(��Ʒ.Length)].Copy();
    sp.IsTradable = false;
    sp.NextTradableTime = DateTimeUtility.GetTradableTime();
    user.Inventory.Items.Add(sp);
    exampleCharacter.Equip(sp);
    sp = ��Ʒ[Random.Shared.Next(��Ʒ.Length)].Copy();
    user.Inventory.Items.Add(sp);
    sp = ��Ʒ[Random.Shared.Next(��Ʒ.Length)].Copy();
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

// ����浵
pc.SaveConfig();
//Item[] ���� = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("11")).ToArray();
//Item[] ���� = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("12")).ToArray();
//Item[] Ь�� = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("13")).ToArray();
//Item[] ��Ʒ = FunGameUtil.Equipment.Where(i => i.Id.ToString().StartsWith("14")).ToArray();
//Item? a = null, b = null, c = null, d = null;
//if (����.Length > 0)
//{
//    a = ����[Random.Shared.Next(����.Length)];
//    exampleCharacter.Equip(a.Copy());
//}
//if (����.Length > 0)
//{
//    b = ����[Random.Shared.Next(����.Length)];
//    exampleCharacter.Equip(b.Copy());
//}
//if (Ь��.Length > 0)
//{
//    c = Ь��[Random.Shared.Next(Ь��.Length)];
//    exampleCharacter.Equip(c.Copy());
//}
//if (��Ʒ.Length > 0)
//{
//    d = ��Ʒ[Random.Shared.Next(��Ʒ.Length)];
//    exampleCharacter.Equip(d.Copy());
//    d = ��Ʒ[Random.Shared.Next(��Ʒ.Length)];
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
//Console.WriteLine($"ִ��ʱ�䣺{stopwatch.Elapsed.TotalSeconds} ��");

//IEnumerable<Character> cs = FunGameSimulation.CharacterStatistics.OrderByDescending(d => d.Value.Rating).ThenByDescending(d => d.Value.Winrates).Select(d => d.Key);
//Console.WriteLine("=== ����ģʽ���а� ===");
//foreach (Character c in cs)
//{
//    CharacterStatistics stats = FunGameSimulation.CharacterStatistics[c];
//    StringBuilder builder = new();

//    builder.AppendLine(c.ToString());
//    builder.AppendLine($"���Σ�{stats.Plays}");
//    builder.AppendLine($"ʤ�ʣ�{stats.Winrates * 100:0.##}%");
//    builder.AppendLine($"�����÷֣�{stats.Rating:0.0#}");

//    Console.WriteLine(builder.ToString());
//}

//IEnumerable<Character> cs2 = FunGameSimulation.TeamCharacterStatistics.OrderByDescending(d => d.Value.Rating).ThenByDescending(d => d.Value.Winrates).Select(d => d.Key);
//Console.WriteLine("=== �Ŷ�ģʽ���а� ===");
//foreach (Character c in cs2)
//{
//    CharacterStatistics stats = FunGameSimulation.TeamCharacterStatistics[c];
//    StringBuilder builder = new();

//    builder.AppendLine(c.ToString());
//    builder.AppendLine($"���Σ�{stats.Plays}");
//    builder.AppendLine($"ʤ�ʣ�{stats.Winrates * 100:0.##}%");
//    builder.AppendLine($"�����÷֣�{stats.Rating:0.0#}");

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
                builder.AppendLine($"�ܼ�����˺���{stats.TotalDamage:0.##} / ������{stats.AvgDamage:0.##}");
                builder.AppendLine($"�ܼ���������˺���{stats.TotalPhysicalDamage:0.##} / ������{stats.AvgPhysicalDamage:0.##}");
                builder.AppendLine($"�ܼ����ħ���˺���{stats.TotalMagicDamage:0.##} / ������{stats.AvgMagicDamage:0.##}");
                builder.AppendLine($"�ܼ������ʵ�˺���{stats.TotalRealDamage:0.##} / ������{stats.AvgRealDamage:0.##}");
                builder.AppendLine($"�ܼƳ����˺���{stats.TotalTakenDamage:0.##} / ������{stats.AvgTakenDamage:0.##}");
                builder.AppendLine($"�ܼƳ��������˺���{stats.TotalTakenPhysicalDamage:0.##} / ������{stats.AvgTakenPhysicalDamage:0.##}");
                builder.AppendLine($"�ܼƳ���ħ���˺���{stats.TotalTakenMagicDamage:0.##} / ������{stats.AvgTakenMagicDamage:0.##}");
                builder.AppendLine($"�ܼƳ�����ʵ�˺���{stats.TotalTakenRealDamage:0.##} / ������{stats.AvgTakenRealDamage:0.##}");
                builder.AppendLine($"�ܼƴ��غ�����{stats.LiveRound} / ������{stats.AvgLiveRound}");
                builder.AppendLine($"�ܼ��ж��غ�����{stats.ActionTurn} / ������{stats.AvgActionTurn}");
                builder.AppendLine($"�ܼƴ��ʱ����{stats.LiveTime:0.##} / ������{stats.AvgLiveTime:0.##}");
                builder.AppendLine($"�ܼ�׬ȡ��Ǯ��{stats.TotalEarnedMoney} / ������{stats.AvgEarnedMoney}");
                builder.AppendLine($"ÿ�غ��˺���{stats.DamagePerRound:0.##}");
                builder.AppendLine($"ÿ�ж��غ��˺���{stats.DamagePerTurn:0.##}");
                builder.AppendLine($"ÿ���˺���{stats.DamagePerSecond:0.##}");
                builder.AppendLine($"�ܼƻ�ɱ����{stats.Kills}" + (stats.Plays != 0 ? $" / ������{(double)stats.Kills / stats.Plays:0.##}" : ""));
                builder.AppendLine($"�ܼ���������{stats.Deaths}" + (stats.Plays != 0 ? $" / ������{(double)stats.Deaths / stats.Plays:0.##}" : ""));
                builder.AppendLine($"�ܼ���������{stats.Assists}" + (stats.Plays != 0 ? $" / ������{(double)stats.Assists / stats.Plays:0.##}" : ""));
                builder.AppendLine($"�ܼ���ɱ����{stats.FirstKills}" + (stats.Plays != 0 ? $" / ��ɱ�ʣ�{(double)stats.FirstKills / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"�ܼ���������{stats.FirstDeaths}" + (stats.Plays != 0 ? $" / �����ʣ�{(double)stats.FirstDeaths / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"�ܼƲ�������{stats.Plays}");
                builder.AppendLine($"�ܼƹھ�����{stats.Wins}");
                builder.AppendLine($"�ܼ�ǰ������{stats.Top3s}");
                builder.AppendLine($"�ܼưܳ�����{stats.Loses}");

                List<string> names = [.. FunGameSimulation.TeamCharacterStatistics.OrderByDescending(kv => kv.Value.Winrates).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"ʤ�ʣ�{stats.Winrates * 100:0.##}%��#{names.IndexOf(character.GetName()) + 1}��");

                names = [.. FunGameSimulation.TeamCharacterStatistics.OrderByDescending(kv => kv.Value.Rating).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"�����÷֣�{stats.Rating:0.##}��#{names.IndexOf(character.GetName()) + 1}��");

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
                builder.AppendLine($"�ܼ�����˺���{stats.TotalDamage:0.##} / ������{stats.AvgDamage:0.##}");
                builder.AppendLine($"�ܼ���������˺���{stats.TotalPhysicalDamage:0.##} / ������{stats.AvgPhysicalDamage:0.##}");
                builder.AppendLine($"�ܼ����ħ���˺���{stats.TotalMagicDamage:0.##} / ������{stats.AvgMagicDamage:0.##}");
                builder.AppendLine($"�ܼ������ʵ�˺���{stats.TotalRealDamage:0.##} / ������{stats.AvgRealDamage:0.##}");
                builder.AppendLine($"�ܼƳ����˺���{stats.TotalTakenDamage:0.##} / ������{stats.AvgTakenDamage:0.##}");
                builder.AppendLine($"�ܼƳ��������˺���{stats.TotalTakenPhysicalDamage:0.##} / ������{stats.AvgTakenPhysicalDamage:0.##}");
                builder.AppendLine($"�ܼƳ���ħ���˺���{stats.TotalTakenMagicDamage:0.##} / ������{stats.AvgTakenMagicDamage:0.##}");
                builder.AppendLine($"�ܼƳ�����ʵ�˺���{stats.TotalTakenRealDamage:0.##} / ������{stats.AvgTakenRealDamage:0.##}");
                builder.AppendLine($"�ܼƴ��غ�����{stats.LiveRound} / ������{stats.AvgLiveRound}");
                builder.AppendLine($"�ܼ��ж��غ�����{stats.ActionTurn} / ������{stats.AvgActionTurn}");
                builder.AppendLine($"�ܼƴ��ʱ����{stats.LiveTime:0.##} / ������{stats.AvgLiveTime:0.##}");
                builder.AppendLine($"�ܼ�׬ȡ��Ǯ��{stats.TotalEarnedMoney} / ������{stats.AvgEarnedMoney}");
                builder.AppendLine($"ÿ�غ��˺���{stats.DamagePerRound:0.##}");
                builder.AppendLine($"ÿ�ж��غ��˺���{stats.DamagePerTurn:0.##}");
                builder.AppendLine($"ÿ���˺���{stats.DamagePerSecond:0.##}");
                builder.AppendLine($"�ܼƻ�ɱ����{stats.Kills}" + (stats.Plays != 0 ? $" / ������{(double)stats.Kills / stats.Plays:0.##}" : ""));
                builder.AppendLine($"�ܼ���������{stats.Deaths}" + (stats.Plays != 0 ? $" / ������{(double)stats.Deaths / stats.Plays:0.##}" : ""));
                builder.AppendLine($"�ܼ���������{stats.Assists}" + (stats.Plays != 0 ? $" / ������{(double)stats.Assists / stats.Plays:0.##}" : ""));
                builder.AppendLine($"�ܼ���ɱ����{stats.FirstKills}" + (stats.Plays != 0 ? $" / ��ɱ�ʣ�{(double)stats.FirstKills / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"�ܼ���������{stats.FirstDeaths}" + (stats.Plays != 0 ? $" / �����ʣ�{(double)stats.FirstDeaths / stats.Plays * 100:0.##}%" : ""));
                builder.AppendLine($"�ܼƲ�������{stats.Plays}");
                builder.AppendLine($"�ܼƹھ�����{stats.Wins}");
                builder.AppendLine($"�ܼ�ǰ������{stats.Top3s}");
                builder.AppendLine($"�ܼưܳ�����{stats.Loses}");

                List<string> names = [.. FunGameSimulation.CharacterStatistics.OrderByDescending(kv => kv.Value.Winrates).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"ʤ�ʣ�{stats.Winrates * 100:0.##}%��#{names.IndexOf(character.GetName()) + 1}��");
                builder.AppendLine($"ǰ���ʣ�{stats.Top3rates * 100:0.##}%");

                names = [.. FunGameSimulation.CharacterStatistics.OrderByDescending(kv => kv.Value.Rating).Select(kv => kv.Key.GetName())];
                builder.AppendLine($"�����÷֣�{stats.Rating:0.##}��#{names.IndexOf(character.GetName()) + 1}��");
                builder.AppendLine($"�ϴ�������{stats.LastRank} / �������Σ�{stats.AvgRank}");

                Console.WriteLine(builder.ToString());
            }
        }
    }
    input = Console.ReadLine() ?? "";
}
