using System.Text;
using Microsoft.Extensions.Logging;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Testing.Tests;
using Oshima.FunGame.OshimaModules;
using Oshima.FunGame.OshimaModules.Regions;
using Oshima.FunGame.OshimaServers.Service;
using Oshima.FunGame.WebAPI.Controllers;

//_ = new Milimoe.FunGame.Testing.Solutions.Novels();

//_ = new Milimoe.FunGame.Testing.Tests.CheckDLL();

Console.WriteLine();

//_ = new Milimoe.FunGame.Testing.Tests.WebSocketTest();

CharacterModule cm = new();
cm.Load();
SkillModule sm = new();
sm.Load();
ItemModule im = new();
im.Load();

FunGameService.InitFunGame();
FunGameSimulation.InitFunGameSimulation();
FunGameController controller = new(new Logger<FunGameController>(new LoggerFactory()));

//await CharacterTest.CharacterTest1();
//foreach (Character c in FunGameConstant.Characters)
//{
//    Character character = c.Copy();
//    character.Recovery();
//    FunGameService.AddCharacterSkills(character, 1, 1, 1);
//    Console.WriteLine(character.GetInfo());
//}
//foreach (Skill s in FunGameConstant.Skills)
//{
//    s.Level = 1;
//    Console.WriteLine(s.GetInfo());
//}
//foreach (Skill m in FunGameConstant.Magics)
//{
//    m.Level = 1;
//    Console.WriteLine(m.GetInfo());
//}
//foreach (Character c in FunGameConstant.Characters)
//{
//    Character character = c.Copy();
//    character.Level = 60;
//    character.Recovery();
//    FunGameService.AddCharacterSkills(character, 1, 6, 6);
//    Console.WriteLine(character.GetInfo());
//}
//foreach (Skill s in FunGameConstant.Skills)
//{
//    s.Level = 6;
//    Console.WriteLine(s.GetInfo());
//}
//foreach (Skill m in FunGameConstant.Magics)
//{
//    m.Level = 8;
//    Console.WriteLine(m.GetInfo());
//}
//Character character = new Oshima.FunGame.OshimaModules.Characters.CustomCharacter(0, "");
//character.SetLevel(60);
//foreach (Item i in FunGameConstant.Equipment)
//{
//    character.Equip(i);
//    if (i.ItemType == ItemType.GiftBox && i.Name != "毕业礼包") continue;
//    Console.WriteLine(i.ToString());
//}
//Console.WriteLine(character.GetInfo());
foreach (Item i in FunGameConstant.Equipment)
{
    StringBuilder builder = new();

    builder.AppendLine($"【{i.Name}】");

    string itemquality = ItemSet.GetQualityTypeName(i.QualityType);
    string itemtype = ItemSet.GetItemTypeName(i.ItemType) + (i.ItemType == ItemType.Weapon && i.WeaponType != WeaponType.None ? "-" + ItemSet.GetWeaponTypeName(i.WeaponType) : "");
    if (itemtype != "") itemtype = $" {itemtype}";

    builder.AppendLine($"{itemquality + itemtype}");

    if (i.Description != "")
    {
        builder.AppendLine("物品描述：" + i.Description);
    }

    if (i.BackgroundStory != "")
    {
        builder.AppendLine($"\"{i.BackgroundStory}\"");
    }

    Console.WriteLine(builder.ToString());
}
Console.ReadKey();
foreach (OshimaRegion region in FunGameConstant.Regions.Union(FunGameConstant.PlayerRegions))
{
    Console.WriteLine(region.ToString());
}
Console.ReadKey();

//Dictionary<int, RoundRecord> rounds = FunGameSimulation.ReadRoundsFromZip("rounds_archive.zip") ?? [];
//Console.WriteLine(rounds.Count);
//Console.ReadKey();
//rounds.Clear();

//await FunGameBO5.StartBO5();
//Console.ReadKey();

//await FunGameTesting.StartGame(true, false);
//Console.ReadKey();

while (true)
{
    await FunGameSimulation.StartSimulationGame(true, true, true, true, useStore: false);
    Console.ReadKey();
    await FunGameSimulation.StartSimulationGame(true, false, false, true);
    Console.ReadKey();
}

//strings.ForEach(Console.WriteLine);

//_ = new Milimoe.FunGame.Testing.Tests.ActivityExample();

//List<string> strings = await FunGameSimulation.StartSimulationGame(true, false, true);
//strings.ForEach(Console.WriteLine);

//Character testc = new CustomCharacter(1, "1");
//Console.WriteLine(testc.GetInfo());
//Console.WriteLine(testc.InitialSTR + $" ({testc.STRGrowth}/Lv)");
//Console.WriteLine(testc.InitialAGI + $" ({testc.AGIGrowth}/Lv)");
//Console.WriteLine(testc.InitialINT + $" ({testc.INTGrowth}/Lv)");
//Console.WriteLine(controller.GetWinrateRank());
//Console.WriteLine(controller.GetWinrateRank(true));
//Console.WriteLine(controller.GetRatingRank());
//Console.WriteLine(controller.GetRatingRank(true));
//Console.WriteLine(controller.CreateSaved(1, "test1"));
//Console.WriteLine(controller.CreateSaved(2, "test2"));

//PluginConfig pc = new("saved", "2");
//pc.LoadConfig();
//User user = FunGameService.GetUser(pc);
//Console.WriteLine(user.Inventory);
//Character c = user.Inventory.Characters.First();

//Dictionary<string, object> skillargs = [];
//skillargs.Add("active", true);
//skillargs.Add("self", true);
//skillargs.Add("enemy", false);
//Skill skill = Factory.OpenFactory.GetInstance<Skill>((long)EffectID.GetEXP, "¾­ÑéÊé", skillargs);
//Dictionary<string, object> effectargs = new()
//{
//    { "skill", skill },
//    {
//        "values",
//        new Dictionary<string, object>()
//        {
//            { "exp", 7777 }
//        }
//    }
//};
//skill.Effects.Add(Factory.OpenFactory.GetInstance<Effect>(skill.Id, "", effectargs));
//skill.Character = c;
//skill.Level = 1;

//skill.OnSkillCasted(null, c, [c]);
//c.OnLevelUp();
//c.OnLevelBreak();

//Console.WriteLine(user.Inventory.Characters.First().GetInfo(showEXP: true));

//Console.WriteLine(string.Join("", controller.FightCustom(1, 2, true)));

//FunGameActionQueue.StartSimulationGame(true, true, true, true);
//foreach (string str in controller.GetTest(false, true))
//{
//    Console.WriteLine(str);
//}

//¿â´æ²âÊÔ
//PluginConfig pc = new("saved", "1");
//pc.LoadConfig();
//User u = FunGameService.GetUser(pc);
//if (u.Inventory.Characters.Count == 0)
//{
//    u.Inventory.Characters.Add(FunGameService.Characters[0].Copy());
//}
//Character c = u.Inventory.Characters.First();
//Item? i = FunGameService.GenerateMagicCardPack(3);
//if (i != null)
//{
//    u.Inventory.Items.Add(i);
//    c.Equip(i);
//}
//Console.WriteLine(u.Inventory.Characters.First().GetInfo());
//Item? i2 = c.UnEquip(Milimoe.FunGame.Core.Library.Constant.EquipSlotType.MagicCardPack);
//Console.WriteLine(i2);
//pc.Add("user", u);
//pc.SaveConfig();
//pc.LoadConfig();
//u = FunGameService.GetUser(pc);

//for (int i = 1; i <= 100; i++)
//{
//    Console.WriteLine($"{i}´Î£º" + string.Join("\r\n", controller.DrawCards(1)));
//}
//Console.WriteLine(NetworkUtility.JsonDeserialize<string>(controller.GetInventoryInfo(1)));
//Console.WriteLine(string.Join("\r\n", controller.GetInventoryInfo2(1, 2)));

while (true)
{
    try
    {
        string msg = Console.ReadLine() ?? "";
        if (msg == "quit") return;
        if (msg.StartsWith("dhjb"))
        {
            msg = msg.Replace("dhjb", "");
            if (int.TryParse(msg, out int value))
            {
                Console.WriteLine(controller.ExchangeCredits(1, value));
            }
            else Console.WriteLine(controller.ExchangeCredits(1));
        }
        else if (msg.StartsWith("winrate"))
        {
            msg = msg.Replace("winrate", "");
            if (int.TryParse(msg, out int value))
            {
                Console.WriteLine(controller.GetWinrateRank(true));
            }
            else Console.WriteLine(controller.GetWinrateRank(false));
        }
        else if (msg.StartsWith("csj"))
        {
            msg = msg.Replace("csj", "");
            if (int.TryParse(msg, out int value))
            {
                Console.WriteLine(controller.GetStats(value));
            }
            else Console.WriteLine(controller.GetStats(1));
        }
        else if (msg.StartsWith("ctdsj"))
        {
            msg = msg.Replace("ctdsj", "");
            if (int.TryParse(msg, out int value))
            {
                Console.WriteLine(controller.GetTeamStats(value));
            }
            else Console.WriteLine(controller.GetTeamStats(1));
        }
        else if (msg == "jscs")
        {
            Console.WriteLine(controller.RandomCustomCharacter(1));
        }
        else if (msg == "qrjscs")
        {
            Console.WriteLine(controller.RandomCustomCharacter(1, true));
        }
        else if (msg == "kb")
        {
            Console.WriteLine(string.Join("\r\n", controller.GenerateMagicCardPack()));
        }
        else if (msg == "tck")
        {
            Console.WriteLine(string.Join("\r\n", controller.DrawCards(1)));
        }
        else if (msg == "ck")
        {
            Console.WriteLine(controller.DrawCard(1));
        }
        else if (msg == "qk")
        {
            Console.WriteLine(await FunGameService.AllowSellAndTrade());
        }
        else if (msg.StartsWith("sl") && int.TryParse(msg.Replace("sl", ""), out int page1))
        {
            Console.WriteLine(string.Join("\r\n", controller.GetInventoryInfo3(1, page1)));
        }
        else if (msg.StartsWith("pzsl") && int.TryParse(msg.Replace("pzsl", ""), out int page3))
        {
            Console.WriteLine(string.Join("\r\n", controller.GetInventoryInfo3(1, page3, 2, 2)));
        }
        else if (msg.StartsWith("cjs") && int.TryParse(msg.Replace("cjs", ""), out int cIndex))
        {
            Console.WriteLine(NetworkUtility.JsonDeserialize<string>(controller.GetCharacterInfoFromInventory(1, cIndex)));
        }
        else if (msg.StartsWith("cwp") && int.TryParse(msg.Replace("cwp", ""), out int itemIndex))
        {
            Console.WriteLine(NetworkUtility.JsonDeserialize<string>(controller.GetItemInfoFromInventory(1, itemIndex)));
        }
        else if (int.TryParse(msg, out int page2))
        {
            Console.WriteLine(string.Join("\r\n", controller.GetInventoryInfo2(1, page2)));
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}
