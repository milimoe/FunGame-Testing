using Microsoft.Extensions.Logging;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Testing.Tests;
using Oshima.FunGame.OshimaModules;
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

while (true)
{
    await FunGameSimulation.StartSimulationGame(true, false, true, true);
    Console.ReadKey();
}

await FunGameBO5.StartBO5();

//await FunGameTesting.StartGame(true, false);

//strings.ForEach(Console.WriteLine);

//_ = new Milimoe.FunGame.Testing.Tests.ActivityExample();

//List<string> strings = await FunGameSimulation.StartSimulationGame(true, false, true);
//strings.ForEach(Console.WriteLine);

//Character testc = new CustomCharacter(1, "1");
//Console.WriteLine(testc.GetInfo());
//Console.WriteLine(testc.InitialSTR + $" ({testc.STRGrowth}/Lv)");
//Console.WriteLine(testc.InitialAGI + $" ({testc.AGIGrowth}/Lv)");
//Console.WriteLine(testc.InitialINT + $" ({testc.INTGrowth}/Lv)");

FunGameController controller = new(new Logger<FunGameController>(new LoggerFactory()));
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
