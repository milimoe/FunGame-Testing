using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Testing.Solutions;

namespace Milimoe.FunGame.Testing.Tests
{
    public class ActivityExample
    {
        public ActivityExample()
        {
            // 创建活动
            Activity activity = new(1, "新年活动", new DateTime(2025, 1, 1, 0, 0, 0), new DateTime(2025, 1, 15, 23, 59, 59));

            // 添加任务
            Quest continuousQuest = new()
            {
                Id = 1,
                Name = "每日登录",
                Description = "每日登录游戏",
                EstimatedMinutes = 1,
                Awards = new Dictionary<string, int> { { "积分", 10 } },
                QuestType = QuestType.Continuous
            };
            Quest immediateQuest = new()
            {
                Id = 2,
                Name = "新手引导",
                Description = "完成新手引导",
                Awards = new Dictionary<string, int> { { "积分", 20 } },
                QuestType = QuestType.Immediate
            };
            Quest progressiveQuest = new()
            {
                Id = 3,
                Name = "击败Boss",
                Description = "击败Boss",
                Awards = new Dictionary<string, int> { { "积分", 30 } },
                QuestType = QuestType.Progressive,
                MaxProgress = 10,
                Progress = 5
            };
            activity.AddQuest(continuousQuest);
            activity.AddQuest(immediateQuest);
            activity.AddQuest(progressiveQuest);

            // 添加商店物品
            Item item1 = Factory.GetItem();
            item1.Id = 1;
            item1.Name = "金币";
            item1.Description = "游戏金币";
            Item item2 = Factory.GetItem();
            item2.Id = 2;
            item2.Name = "道具";
            item2.Description = "游戏道具";
            activity.Store.AddItem(item1, 100, 10);
            activity.Store.AddItem(item2, 200, 10);

            // 订阅活动状态改变事件
            activity.ActivityStateChanged += (sender, e) =>
            {
                Console.WriteLine($"活动状态已改变为：{e.NewState}");
            };

            // 订阅用户访问检查事件
            activity.UserAccessCheck += (sender, e) =>
            {
                if (e.ActivityState == ActivityState.InProgress)
                {
                    e.AllowAccess = true; // 只有进行中状态才允许访问
                }
                else
                {
                    Console.WriteLine($"用户 {e.UserId} 尝试访问活动，但当前状态为 {e.ActivityState}，不允许访问。");
                }
            };

            // 模拟用户访问
            long userId = 123;
            Points points = new(userId, 30000);
            while (true)
            {
                activity.UpdateState(); // 定时更新活动状态
                if (activity.AllowUserAccess(userId))
                {
                    Console.WriteLine(string.Join("\r\n", activity.Quests));

                    Console.WriteLine($"用户 {userId} 成功访问活动！");
                    // 模拟用户兑换物品
                    RedeemResult result = activity.Store.TryRedeemItem(points, 1);
                    switch (result)
                    {
                        case RedeemResult.Success:
                            Console.WriteLine($"用户 {userId} 成功兑换了 {item1.Name}！");
                            break;
                        case RedeemResult.StockNotEnough:
                            Console.WriteLine($"用户 {userId} 兑换 {item1.Name} 失败，库存不足！");
                            break;
                        case RedeemResult.PointsNotEnough:
                            Console.WriteLine($"用户 {userId} 兑换 {item1.Name} 失败，积分不足！");
                            break;
                    }

                    Console.WriteLine($"用户 {userId} 当前积分：{points.Amount}");

                }

                //Thread.Sleep(1000); // 每秒检查一次
                Console.ReadLine();
            }
        }
    }
}
