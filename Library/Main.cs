using Microsoft.Extensions.Logging;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Oshima.Core.Controllers;
using Oshima.Core.Utils;
using Oshima.FunGame.OshimaModules;

CharacterModule cm = new();
cm.Load();
SkillModule sm = new();
sm.Load();
ItemModule im = new();
im.Load();

FunGameService.InitFunGame();
FunGameSimulation.InitFunGame();

//List<string> strings = FunGameSimulation.StartGame(true, false, true);
//strings.ForEach(Console.WriteLine);

FunGameController controller = new(new Logger<FunGameController>(new LoggerFactory()));
Console.WriteLine(controller.CreateSaved(1, "test"));

//ø‚¥Ê≤‚ ‘
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
//    Console.WriteLine($"{i}¥Œ£∫" + string.Join("\r\n", controller.DrawCards(1)));
//}
//Console.WriteLine(NetworkUtility.JsonDeserialize<string>(controller.GetInventoryInfo(1)));
//Console.WriteLine(string.Join("\r\n", controller.GetInventoryInfo2(1, 2)));

while (true)
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
