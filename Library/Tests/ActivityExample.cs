using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;

namespace Milimoe.FunGame.Testing.Tests
{
    public class ActivityExample
    {
        public ActivityExample()
        {
            DateTime date1 = new(2025, 1, 1, 0, 0, 0);
            DateTime date1End = new(2025, 1, 15, 23, 59, 59);
            DateTime date2 = new(2025, 1, 16, 0, 0, 0);
            DateTime date2End = new(2025, 1, 31, 23, 59, 59);
            DateTime date3 = new(2025, 2, 1, 0, 0, 0);
            DateTime date3End = new(2025, 2, 9, 23, 59, 59);

            for (int i = 1; i <= 3; i++)
            {
                Activity activity;
                // 创建活动
                if (i == 1)
                {
                    activity = new(i, $"新年活动-阶段{i}", date1, date1End);
                }
                else if (i == 2)
                {
                    activity = new(i, $"新年活动-阶段{i}", date2, date2End);
                }
                else
                {
                    activity = new(i, $"新年活动-阶段{i}", date3, date3End);
                }

                // 添加任务，需要注意的是，这些任务是全服玩家参与的任务，而不是每个玩家自己的任务。
                /// 如果需要添加玩家自己的任务，需要做额外工作，可使用 <see cref="EntityModuleConfig{Quest}"> 类。
                Quest quest = new()
                {
                    Id = 1,
                    Name = "击败Boss",
                    Description = "全服玩家累计击败100个Boss",
                    CreditsAward = 10000,
                    MaterialsAward = 800,
                    Awards = [],
                    QuestType = QuestType.Progressive,
                    MaxProgress = 100
                };
                activity.Quests.Add(quest);

                // 纳入活动中心管理
                EventCenter.Instance.AddActivity("新年活动", activity);
            }

            // 订阅玩家获取活动信息事件
            EventCenter.Instance.RegisterUserGetActivityInfoEventHandler("新年活动", e =>
            {
                // 玩家获取活动信息时，需要显示对应其账号的任务信息
                // 首先，使用 EntityModuleConfig 创建一个存档管理器
                EntityModuleConfig<Quest> quests = new(e.Activity.Name, e.UserId.ToString());
                // 获取任务列表，如果没有，我们需要为之生成
                quests.LoadConfig();
                if (quests.Count == 0)
                {
                    GenerateQuest(quests);
                }
                SyncQuestStateToActivity(e.Activity, quests);
                // 先显示活动的信息
                Console.WriteLine(e.Activity.ToString(false));
                // 再显示任务信息
                List<Quest> list = new(e.Activity.Quests);
                list.AddRange(quests.Values);
                Console.WriteLine("=== 任务列表 ===");
                Console.WriteLine(string.Join("\r\n", list));
            });

            // 订阅用户访问检查事件
            EventCenter.Instance.RegisterUserAccessEventHandler("新年活动", e =>
            {
                if (e.ActivityState == ActivityState.InProgress)
                {
                    // 只有进行中状态才允许访问
                    e.AllowAccess = true;
                    // 实现活动逻辑，例如触发打BOSS任务
                    if (e.QuestId == 1 && e.Activity.Quests.FirstOrDefault(q => q.Id == 1) is Quest quest)
                    {
                        if (quest.Status == QuestState.InProgress)
                        {
                            if (Random.Shared.Next(3) > 1)
                            {
                                Console.WriteLine($"用户 {e.UserId} 挑战 Boss 成功！");
                                quest.Progress++;
                                if (quest.Progress >= quest.MaxProgress)
                                {
                                    quest.Progress = quest.MaxProgress;
                                    quest.Status = QuestState.Completed;
                                    Console.WriteLine($"任务 [ {quest.Name} ] 完成！全服玩家将获得奖励【{quest.AwardsString}】！");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"用户 {e.UserId} 挑战 Boss 失败，下次再来吧。");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"用户 {e.UserId} 尝试做任务 [ {quest.Name} ] ，但是这个任务的状态为 {CommonSet.GetQuestStatus(quest.Status)} ，做任务失败！");
                        }
                    }
                    // 接上文，开发者可以在此事件中创建 EntityModuleConfig 对象来为玩家定制任务
                    // 现在，我们判断当用户访问的不是全服活动时，如何处理定制任务
                    if (e.QuestId != 1)
                    {
                        // 首先，使用 EntityModuleConfig 创建一个存档管理器
                        EntityModuleConfig<Quest> quests = new(e.Activity.Name, e.UserId.ToString());
                        // 获取任务列表，如果没有，我们需要为之生成
                        quests.LoadConfig();
                        if (quests.Count == 0)
                        {
                            GenerateQuest(quests);
                        }
                        SyncQuestStateToActivity(e.Activity, quests);
                        // 找任务
                        if (quests.Values.FirstOrDefault(q => q.Id == e.QuestId) is Quest quest2)
                        {
                            // 写任务的逻辑
                            Console.WriteLine($"用户 {e.UserId} 尝试做任务 [ {quest2.Name} ]");
                            if (quest2.QuestType == QuestType.Continuous)
                            {
                                // 注意：持续性任务比较特别，需要在持续时间结束后自动结算
                                Task.Run(async () =>
                                {
                                    await Task.Delay(1000 * 60 * quest2.EstimatedMinutes);
                                    // 需要做特殊处理
                                });
                            }
                            // 做了任务后要保存
                            quests.SaveConfig();
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"用户 {e.UserId} 尝试访问活动，但当前状态为 {e.ActivityState}，不允许访问。");
                }
            });

            // 模拟用户访问
            long userId = 123;
            string input = Console.ReadLine() ?? "";
            while (input.Trim() != "quit")
            {
                if (input == "1")
                {
                    Activity? a = EventCenter.Instance.GetActivity("新年活动", "新年活动-阶段1");
                    if (a != null && a.AllowUserAccess(userId))
                    {
                        Console.WriteLine($"用户 {userId} 成功访问活动 {a.Name}！");
                    }
                }
                else if (input == "2")
                {
                    Activity? a = EventCenter.Instance.GetActivity("新年活动", "新年活动-阶段2");
                    if (a != null && a.AllowUserAccess(userId))
                    {
                        Console.WriteLine($"用户 {userId} 成功访问活动 {a.Name}！");
                    }
                }
                else if (input == "31")
                {
                    Activity? a = EventCenter.Instance.GetActivity("新年活动", "新年活动-阶段3");
                    if (a != null && a.AllowUserAccess(userId, 1))
                    {
                        Console.WriteLine($"用户 {userId} 成功访问活动 {a.Name} 和任务1！");
                    }
                }
                else if (input == "32")
                {
                    Activity? a = EventCenter.Instance.GetActivity("新年活动", "新年活动-阶段3");
                    if (a != null && a.AllowUserAccess(userId, 2))
                    {
                        Console.WriteLine($"用户 {userId} 成功访问活动 {a.Name} 和任务2！");
                    }
                }
                else if (input == "33")
                {
                    Activity? a = EventCenter.Instance.GetActivity("新年活动", "新年活动-阶段3");
                    if (a != null && a.AllowUserAccess(userId, 3))
                    {
                        Console.WriteLine($"用户 {userId} 成功访问活动 {a.Name} 和任务3！");
                    }
                }
                else if (input == "4")
                {
                    // 显示所有活动
                    Console.WriteLine($"新年活动正在火热进行中！");
                    foreach (Activity activity in EventCenter.Instance["新年活动"])
                    {
                        activity.GetActivityInfo(userId);
                    }
                    Store? store = EventCenter.Instance.GetStore("新年活动");
                    if (store != null)
                    {
                        Console.WriteLine($"新年活动商店：[ {store.Name} ]，请游玩活动项目后进入商店兑换纪念品！");
                    }
                }
                input = Console.ReadLine() ?? "";
            }

            EventCenter.Instance.UnRegisterUserAccess("新年活动");
            EventCenter.Instance.UnRegisterUserGetActivityInfo("新年活动");
        }

        private void GenerateQuest(EntityModuleConfig<Quest> quests)
        {
            // 生成任务
            quests.Add("签到", new()
            {
                Id = 2,
                Name = "签到",
                Description = "每日登录游戏即可完成签到",
                CreditsAward = 10,
                MaterialsAward = 10,
                Awards = [],
                QuestType = QuestType.Immediate
            });
            quests.Add("新手引导", new()
            {
                Id = 3,
                Name = "新手引导",
                Description = "完成新手引导",
                EstimatedMinutes = 1,
                CreditsAward = 10,
                MaterialsAward = 10,
                Awards = [],
                QuestType = QuestType.Continuous
            });
            // 保存，下次会读取
            quests.SaveConfig();
        }

        private void SyncQuestStateToActivity(Activity activity, EntityModuleConfig<Quest> quests)
        {
            // 使任务的状态与活动同步
            foreach (Quest quest in quests.Values)
            {
                if (activity.Status == ActivityState.InProgress)
                {
                    // 只有进度式任务需要修改进行状态，其他类型是开始任务时修改
                    if (quest.Status == QuestState.NotStarted && quest.QuestType == QuestType.Progressive)
                    {
                        quest.Status = QuestState.InProgress;
                    }
                }
                else if (activity.Status == ActivityState.Ended)
                {
                    // 活动结束了，还没开始或者没做完的任务就不能再做了，标记为未完成
                    if (quest.Status == QuestState.NotStarted || quest.Status == QuestState.InProgress)
                    {
                        quest.Status = QuestState.Missed;
                    }
                }
            }
            // 保存，下次会读取
            quests.SaveConfig();
        }
    }
}
