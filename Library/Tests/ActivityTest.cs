using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.FunGame.OshimaModules.Items;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    public class ActivityTest
    {
        public static async Task Test()
        {
            await Task.CompletedTask;
            // 创建一个活动实例
            Activity activity1 = new(1, "重构计划特别行动：里程碑Ⅰ")
            {
                Description = "在筽祀牻大陆的能量节点剧烈波动之际，悖论引擎的失控引发了现实法则的紊乱！" +
                    "来自铎京的冒险者们需联手作战，探索大陆各区域，收集关键材料，击败区域头目，修复能量节点，阻止现实崩塌！共同推进“重构计划”，解锁丰厚奖励！\r\n" +
                    "全服玩家需要探索指定区域，收集基础材料，稳定工坊，推进“里程碑Ⅰ”的进度。"
            };
            Quest quest1 = new()
            {
                Id = 1,
                Name = "里程碑Ⅰ·永燃坩埚",
                Description = "收集 2500 份 [ 活体金属苔藓 ]。",
                RegionId = 3,
                NeedyExploreItemName = "活体金属苔藓",
                CreditsAward = 50000,
                Awards = [
                    new 魔法卡礼包(QualityType.Blue, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2500,
                Status = QuestState.InProgress
            };
            Quest quest2 = new()
            {
                Id = 2,
                Name = "里程碑Ⅰ·齿轮坟场",
                Description = "收集 2000 份 [ 机械核心碎片 ]。",
                RegionId = 5,
                NeedyExploreItemName = "机械核心碎片",
                CreditsAward = 40000,
                Awards = [
                    new 魔法卡礼包(QualityType.Blue, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2000,
                Status = QuestState.InProgress
            };
            activity1.Quests.Add(quest1);
            activity1.Quests.Add(quest2);
            Activity activity2 = new(2, "重构计划特别行动：里程碑Ⅱ")
            {
                Description = "在筽祀牻大陆的能量节点剧烈波动之际，悖论引擎的失控引发了现实法则的紊乱！" +
                    "来自铎京的冒险者们需联手作战，探索大陆各区域，收集关键材料，击败区域头目，修复能量节点，阻止现实崩塌！共同推进“重构计划”，解锁丰厚奖励！\r\n" +
                    "全服玩家需要探索指定区域，收集高级材料，稳定林海和荒漠，推进“里程碑Ⅱ”的进度。",
                Predecessor = 1
            };
            Quest quest3 = new()
            {
                Id = 1,
                Name = "里程碑Ⅱ·瑟兰薇歌林海",
                Description = "收集 2500 份 [ 荧蓝汁液 ]。",
                RegionId = 2,
                NeedyExploreItemName = "荧蓝汁液",
                CreditsAward = 50000,
                Awards = [
                    new 魔法卡礼包(QualityType.Purple, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2500
            };
            Quest quest4 = new()
            {
                Id = 2,
                Name = "里程碑Ⅱ·时之荒漠",
                Description = "收集 2000 份 [ 时间碎片 ]。",
                RegionId = 7,
                NeedyExploreItemName = "时间碎片",
                CreditsAward = 40000,
                Awards = [
                    new 魔法卡礼包(QualityType.Purple, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2000
            };
            activity2.Quests.Add(quest3);
            activity2.Quests.Add(quest4);
            Activity activity3 = new(3, "重构计划特别行动：里程碑Ⅲ")
            {
                Description = "在筽祀牻大陆的能量节点剧烈波动之际，悖论引擎的失控引发了现实法则的紊乱！" +
                    "来自铎京的冒险者们需联手作战，探索大陆各区域，收集关键材料，击败区域头目，修复能量节点，阻止现实崩塌！共同推进“重构计划”，解锁丰厚奖励！\r\n" +
                    "全服玩家需要挑战银辉城，获取悖论引擎核心材料，推进“里程碑Ⅲ”的进度。",
                Predecessor = 2
            };
            Quest quest5 = new()
            {
                Id = 1,
                Name = "里程碑Ⅲ·星辉凝露",
                Description = "收集 2000 份 [ 星辉凝露 ]。",
                RegionId = 1,
                NeedyExploreItemName = "星辉凝露",
                CreditsAward = 50000,
                Awards = [
                    new 魔法卡礼包(QualityType.Orange, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2000
            };
            Quest quest6 = new()
            {
                Id = 2,
                Name = "里程碑Ⅲ·液态月光",
                Description = "收集 2000 份 [ 液态月光 ]。",
                RegionId = 1,
                NeedyExploreItemName = "液态月光",
                CreditsAward = 50000,
                Awards = [
                    new 魔法卡礼包(QualityType.Orange, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2000
            };
            activity3.Quests.Add(quest5);
            activity3.Quests.Add(quest6);
            Activity activity4 = new(4, "重构计划特别行动：里程碑Ⅳ")
            {
                Description = "在筽祀牻大陆的能量节点剧烈波动之际，悖论引擎的失控引发了现实法则的紊乱！" +
                    "来自铎京的冒险者们需联手作战，探索大陆各区域，收集关键材料，击败区域头目，修复能量节点，阻止现实崩塌！共同推进“重构计划”，解锁丰厚奖励！\r\n" +
                    "全服玩家需要探索永霜裂痕与棱镜骨桥，稳定时空法则，推进“里程碑Ⅳ”的进度。",
                Predecessor = 3
            };
            Quest quest7 = new()
            {
                Id = 1,
                Name = "里程碑Ⅳ·永霜裂痕",
                Description = "收集 2500 份 [ 时霜药剂 ]。",
                RegionId = 4,
                NeedyExploreItemName = "时霜药剂",
                CreditsAward = 80000,
                Awards = [
                    new 魔法卡礼包(QualityType.Red, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2500
            };
            Quest quest8 = new()
            {
                Id = 2,
                Name = "里程碑Ⅳ·棱镜骨桥",
                Description = "收集 2500 份 [ 晶化记忆孢子 ]。",
                RegionId = 11,
                NeedyExploreItemName = "晶化记忆孢子",
                CreditsAward = 80000,
                Awards = [
                    new 魔法卡礼包(QualityType.Red, 5)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2500
            };
            activity4.Quests.Add(quest7);
            activity4.Quests.Add(quest8);
            Activity activity5 = new(5, "重构计划特别行动：终幕")
            {
                Description = "在筽祀牻大陆的能量节点剧烈波动之际，悖论引擎的失控引发了现实法则的紊乱！" +
                    "来自铎京的冒险者们需联手作战，探索大陆各区域，收集关键材料，击败区域头目，修复能量节点，阻止现实崩塌！共同推进“重构计划”，解锁丰厚奖励！\r\n" +
                    "全服玩家需要收集星银合金，修复悖论引擎，彻底稳定现实。",
                Predecessor = 4
            };
            Quest quest9 = new()
            {
                Id = 1,
                Name = "终幕·悖论挑战",
                Description = "战胜 500 次活动限定 [ 失控的悖论引擎 ]。",
                RegionId = 1,
                NeedyExploreCharacterName = "活动限定失控的悖论引擎",
                CreditsAward = 100000,
                Awards = [
                    new 魔法卡礼包(QualityType.Gold, 3)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 500,
            };
            Quest quest10 = new()
            {
                Id = 2,
                Name = "终幕·星银合金",
                Description = "收集 2000 份 [ 星银合金 ]。",
                RegionId = 1,
                NeedyExploreItemName = "星银合金",
                CreditsAward = 80000,
                Awards = [
                    new 魔法卡礼包(QualityType.Red, 6)
                ],
                AwardsCount = new() {
                    { "魔法卡礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 2000
            };
            activity5.Quests.Add(quest9);
            activity5.Quests.Add(quest10);
            Activity activity6 = new(6, "毕业季", new DateTime(2025, 6, 1, 10, 0, 0), new DateTime(2025, 8, 1, 3, 59, 59))
            {
                Description = "毕业季特别通告：启程既是毕业，大量秘宝馈赠！发送【毕业礼包】指令即可获得毕业礼包一份，礼包包含从零开始拉满一个角色的所有资源，让你的角色直接“毕业”！礼包最多可以领取 2 次。"
            };
            Console.WriteLine(FunGameService.AddEvent(activity1));
            Console.WriteLine(FunGameService.AddEvent(activity2));
            Console.WriteLine(FunGameService.AddEvent(activity3));
            Console.WriteLine(FunGameService.AddEvent(activity4));
            Console.WriteLine(FunGameService.AddEvent(activity5));
            Console.WriteLine(FunGameService.AddEvent(activity6));
            Console.WriteLine();
            Console.WriteLine(FunGameService.GetEventCenter(null));
            Console.WriteLine();
            Console.WriteLine(FunGameService.GetEvents(null));
            Console.WriteLine();
            Console.WriteLine(FunGameService.GetEvent(null, 1));
            Console.WriteLine(FunGameService.GetEvent(null, 2));
            Console.WriteLine(FunGameService.GetEvent(null, 3));
            Console.WriteLine(FunGameService.GetEvent(null, 4));
            Console.WriteLine(FunGameService.GetEvent(null, 5));
            Console.WriteLine(FunGameService.GetEvent(null, 6));
            Console.ReadKey();
        }

        public static void Test2()
        {
            while (true)
            {
                EntityModuleConfig<Quest> quests = new("1", "1");
                Console.WriteLine(FunGameService.CheckQuestList(quests));
                ConsoleKey key = Console.ReadKey().Key;
                if (key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }

        public static void Test3()
        {
            Activity activity = new(1, "糖糖一周年纪念活动", new DateTime(2025, 12, 25, 4, 0, 0), new DateTime(2026, 1, 4, 3, 59, 59))
            {
                Description = "在活动期间，累计消耗 360 个探索许可即可领取【一周年纪念礼包】，打开后获得金币、钻石奖励以及【一周年纪念套装】（包含武器粉糖雾蝶 * 1，防具糖之誓约 * 1，鞋子蜜步流心 * 1，饰品回忆糖纸 * 1，饰品蜂糖蜜酿 * 1）！自2024年12月进入上线前的测试阶段起，糖糖已经陪我们走过了第一个年头，放眼未来，糖糖将为我们带来更多快乐。"
            };
            Quest quest1 = new()
            {
                Id = 1,
                Name = "糖糖一周年纪念",
                Description = "消耗 360 个探索许可（即参与探索玩法、秘境挑战）。",
                NeedyExploreItemName = "探索许可",
                CreditsAward = 10000,
                Awards = [
                    new 一周年纪念礼包()
                ],
                AwardsCount = new() {
                    { "一周年纪念礼包", 1 }
                },
                QuestType = QuestType.Progressive,
                MaxProgress = 360
            };
            activity.Quests.Add(quest1);
            Console.WriteLine(FunGameService.AddEvent(activity));
            Console.WriteLine(FunGameService.GetEventCenter(null));
            Console.WriteLine();
            Console.WriteLine(FunGameService.GetEvents(null));
            Console.WriteLine();
            Console.WriteLine(FunGameService.GetEvent(null, 1));
        }
    }
}
