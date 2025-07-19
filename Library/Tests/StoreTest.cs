using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.FunGame.OshimaModules.Items;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    public class StoreTest
    {
        public static void StoreTest1()
        {
            Store store1 = new("探索者协会后勤部")
            {
                AutoRefresh = true,
                RefreshInterval = 1,
                NextRefreshDate = DateTime.Today.AddHours(4),
            };
            Item item = new 探索许可();
            store1.AddItem(item, 420);
            store1.SetPrice(1, General.GameplayEquilibriumConstant.InGameMaterial, 10);
            item = new 魔法卡礼包(QualityType.White, 4)
            {
                Price = 1200
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Green, 4)
            {
                Price = 8000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Blue, 4)
            {
                Price = 20000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Purple, 4)
            {
                Price = 70000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Orange, 4)
            {
                Price = 160000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Red, 4)
            {
                Price = 310000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Gold, 4)
            {
                Price = 600000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            Store store2 = new("铎京武器商会")
            {
                AutoRefresh = true,
                RefreshInterval = 3,
                NextRefreshDate = DateTime.Today.AddHours(4),
            };
            item = FunGameConstant.Equipment.First(i => i.Id == 11574).Copy();
            item.Price = 120000;
            store2.AddItem(item, 4);
            item = FunGameConstant.Equipment.First(i => i.Id == 11572).Copy();
            item.Price = 60000;
            store2.AddItem(item, 4);
            item = new 攻击之爪85()
            {
                Price = 100000
            };
            store2.AddItem(item, 4);
            item = new 攻击之爪70()
            {
                Price = 55000
            };
            store2.AddItem(item, 4);
            Store store3 = new("小雪杂货铺")
            {
                AutoRefresh = true,
                RefreshInterval = 3,
                StartTime = new DateTime(2025, 7, 21, 8, 0, 0),
                EndTime = new DateTime(2025, 7, 28, 03, 59, 59),
                NextRefreshDate = DateTime.Today.AddHours(4),
            };
            item = FunGameConstant.Equipment.First(i => i.Id == 14510).Copy();
            item.Price = 28000;
            store3.AddItem(item, 20);
            item = FunGameConstant.Equipment.First(i => i.Id == 14511).Copy();
            item.Price = 28000;
            store3.AddItem(item, 20);
            item = FunGameConstant.Equipment.First(i => i.Id == 14512).Copy();
            item.Price = 28000;
            store3.AddItem(item, 20);
            item = FunGameConstant.Equipment.First(i => i.Id == 14513).Copy();
            item.Price = 28000;
            store3.AddItem(item, 20);
            item = FunGameConstant.Equipment.First(i => i.Id == 14514).Copy();
            item.Price = 28000;
            store3.AddItem(item, 20);
            EntityModuleConfig<Store> dokyoStores = new("stores", "dokyo")
            {
                { "dokyo_logistics", store1 },
                { "dokyo_weapons", store2 },
                { "dokyo_yuki", store3 },
            };
            dokyoStores.SaveConfig();
            Console.WriteLine("读取中……");
            dokyoStores.LoadConfig();
            Store? test1 = dokyoStores.Get("dokyo_logistics");
            Store? test2 = dokyoStores.Get("dokyo_weapons");
            Store? test3 = dokyoStores.Get("dokyo_yuki");
            Console.WriteLine(test1);
            Console.WriteLine(test2);
            Console.WriteLine(test3);
            Console.ReadKey();
        }
    }
}
