using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.FunGame.OshimaModules.Items;
using Oshima.FunGame.OshimaModules.Models;

namespace Milimoe.FunGame.Testing.Tests
{
    public class StoreTest
    {
        public static void StoreTest1()
        {
            Store store1 = new("探索者协会后勤部")
            {
                Description = "后勤部发布常驻探索许可，以及提供七折优惠的魔法卡礼包，欢迎广大探索者选购！",
                AutoRefresh = true,
                RefreshInterval = 1,
                NextRefreshDate = DateTime.Today.AddHours(4),
                GetNewerGoodsOnVisiting = true,
            };
            Item item = new 探索许可();
            store1.AddItem(item, 420);
            store1.SetPrice(1, General.GameplayEquilibriumConstant.InGameMaterial, 5);
            item = new 魔法卡礼包(QualityType.White, 4)
            {
                Price = 1500
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
                Price = 63000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Orange, 4)
            {
                Price = 131000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Red, 4)
            {
                Price = 254000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            item = new 魔法卡礼包(QualityType.Gold, 4)
            {
                Price = 470000
            };
            store1.AddItem(item, -1, ItemSet.GetQualityTypeName(item.QualityType) + item.Name);
            store1.CopyGoodsToNextRefreshGoods();
            Store store2 = new("铎京武器商会")
            {
                AutoRefresh = true,
                RefreshInterval = 3,
                NextRefreshDate = DateTime.Today.AddHours(4),
                GetNewerGoodsOnVisiting = true,
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
            store2.CopyGoodsToNextRefreshGoods();
            Store store3 = new("小雪杂货铺")
            {
                AutoRefresh = true,
                RefreshInterval = 3,
                StartTime = new DateTime(2025, 7, 21, 8, 0, 0),
                EndTime = new DateTime(2025, 7, 28, 03, 59, 59),
                StartTimeOfDay = new DateTime(2025, 7, 21, 8, 0, 0),
                EndTimeOfDay = new DateTime(2025, 7, 28, 03, 59, 59),
                NextRefreshDate = DateTime.Today.AddHours(4),
                GetNewerGoodsOnVisiting = true,
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
            item = new 改名卡();
            store3.AddItem(item, 5);
            store3.SetPrice(6, General.GameplayEquilibriumConstant.InGameMaterial, 300);
            store3.CopyGoodsToNextRefreshGoods();
            Store store4 = new("铎京慈善基金会")
            {
                Description = "铎京慈善基金会是大陆上最大的慈善组织，致力于帮助贫困地区和弱势群体。探索者们可以不定期来这里看看有什么福利可以领取！",
                AutoRefresh = false,
                GlobalStock = true,
            };
            item = new 探索助力礼包()
            {
                Price = 0,
            };
            store4.AddItem(item, -1);
            store4.Goods[1].Quota = 1;
            store4.Goods[1].ExpireTime = new DateTime(2025, 7, 26, 03, 59, 59);
            Store store5 = new("锻造积分商店")
            {
                AutoRefresh = true,
                RefreshInterval = 3,
                NextRefreshDate = DateTime.Today.AddHours(4),
                GlobalStock = true,
            };
            item = new 大师锻造券();
            store5.AddItem(item, -1);
            store5.SetPrice(1, "锻造积分", 400);
            EntityModuleConfig<Store> dokyoStores = new("stores", "dokyo")
            {
                { "dokyo_logistics", store1 },
                { "dokyo_weapons", store2 },
                { "dokyo_yuki", store3 },
                { "dokyo_welfare", store4 },
                { "dokyo_forge", store5 },
            };
            dokyoStores.SaveConfig();
            Console.WriteLine("读取中……");
            dokyoStores.LoadConfig();
            Store? test1 = dokyoStores.Get("dokyo_logistics");
            Store? test2 = dokyoStores.Get("dokyo_weapons");
            Store? test3 = dokyoStores.Get("dokyo_yuki");
            Store? test4 = dokyoStores.Get("dokyo_welfare");
            Store? test5 = dokyoStores.Get("dokyo_forge");
            Console.WriteLine(test1);
            Console.WriteLine(test2);
            Console.WriteLine(test3);
            Console.WriteLine(test4);
            Console.WriteLine(test5);
            Console.ReadKey();
        }
    }
}
