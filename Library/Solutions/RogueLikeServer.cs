using System.Text;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface.Base;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Common.Architecture;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Milimoe.FunGame.Testing.Tests;
using Oshima.FunGame.OshimaModules.Characters;
using Oshima.FunGame.OshimaModules.Models;
using Oshima.FunGame.OshimaModules.Regions;
using Oshima.FunGame.OshimaServers.Service;
using Oshima.FunGame.WebAPI.Controllers;

namespace Milimoe.FunGame.Testing.Solutions
{
    public class RogueLikeServer(RogueLikeDispatcher dispatcher)
    {
        public bool Running => Dispatcher.Running;
        public Task? Guard { get; set; } = null;
        public RogueLikeDispatcher Dispatcher { get; set; } = dispatcher;
        public ConcurrentQueue<(Guid Guid, DataRequestArgs Args)> Inquiries { get; } = [];
        public Dictionary<string, User> Users { get; set; } = [];
        public Dictionary<string, RogueLikeGameData> RogueLikeGameDatas { get; set; } = [];

        public string Credit_Name { get; set; } = General.GameplayEquilibriumConstant.InGameCurrency;
        public string Material_Name { get; set; } = General.GameplayEquilibriumConstant.InGameMaterial;

        private readonly FunGameController _controller = dispatcher.Controller;

        public void ReceiveDataRequest(Guid guid, DataRequestArgs args)
        {
            Inquiries.Add((guid, args));
        }

        public void WriteLine(string message = "") => Dispatcher.WriteLine(message);

        public void AddDialog(string speaker = "", string message = "") => Dispatcher.AddDialog(speaker, message);

        public async Task DataRequestGuard()
        {
            while (Running)
            {
                if (!Inquiries.IsEmpty && Inquiries.GetFirst(out var obj))
                {
                    DataRequestArgs response = await HandleDataRequest(obj.Args);
                    await DataRequestCallback(obj.Guid, response);
                }
                await Task.Delay(50);
            }
        }

        public async Task CreateGameLoop(string username)
        {
            Users.TryGetValue(username, out User? user);
            user ??= General.UnknownUserInstance;
            Character character = user.Inventory.MainCharacter;
            RogueLikeGameData data = new(character);
            RogueLikeGameDatas[username] = data;
            await GameLoop(data);
        }

        private async Task GameLoop(RogueLikeGameData data)
        {
            while (data.RogueState != RogueState.Finish)
            {
                data.RogueState = await NextRogueState(data, data.RogueState);
                if (data.RogueState == RogueState.Init)
                {
                    data.RogueState = RogueState.Finish;
                }
                await Task.Delay(100);
            }
        }

        private async Task<DataRequestArgs> HandleDataRequest(DataRequestArgs args)
        {
            DataRequestArgs response = new(args.RequestType);

            switch (response.RequestType)
            {
                case "createuser":
                    {
                        AddDialog("？？？", "探索者，欢迎入职【永恒方舟计划】，我是您的专属 AI，协助您前往指定任务地点开展勘测工作。请问您的名字是？");
                        string username = "";
                        do
                        {
                            InquiryResponse inquiryResponse = await Dispatcher.GetInquiryResponse(new(InquiryType.TextInput, "请问您的名字是？"));
                            if (inquiryResponse.TextResult == "")
                            {
                                WriteLine("请输入不为空的字符串。");
                            }
                            else
                            {
                                username = inquiryResponse.TextResult;
                            }
                        }
                        while (username == "");
                        long uid = Users.Count + 1;
                        PluginConfig pc = FunGameService.GetUserConfig(uid, out _);
                        User user = Factory.GetUser(uid, username);
                        Character character = new CustomCharacter(uid, username)
                        {
                            Level = 1
                        };
                        character.Recovery();
                        user.Inventory.Credits = 100;
                        user.Inventory.Materials = 5;
                        user.Inventory.Characters.Add(character);
                        user.Inventory.MainCharacter = character;
                        pc.Add("user", user);
                        FunGameService.SetUserConfig(uid.ToString(), pc, user);
                        Users[username] = user;
                        response.Data["user"] = user;
                        AddDialog("柔哥", $"让我再次欢迎您，{username}。入职手续已办理完毕，从今以后，您就是【永恒方舟计划】的探员了。请您记住我的名字：【柔哥】，如有任何需要我都会随时提供您帮助。");
                        break;
                    }
                case "login":
                    {
                        long uid = 0;
                        if (args.Data.TryGetValue("uid", out object? value))
                        {
                            uid = Convert.ToInt64(value);
                        }
                        PluginConfig pc = FunGameService.GetUserConfig(uid, out _);
                        if (pc.ContainsKey("user"))
                        {
                            User user = FunGameService.GetUser(pc);
                            Users[user.Username] = user;
                            response.Data["user"] = user;
                            AddDialog("柔哥", $"{user.Username}，欢迎您回到永恒方舟！");
                        }
                        else WriteLine("登录失败，没有找到符合条件的存档。");
                    }
                    break;
                default:
                    break;
            }

            return response;
        }

        private async Task DataRequestCallback(Guid guid, DataRequestArgs response)
        {
            await Dispatcher.DataRequestComplete(guid, response);
        }

        private async Task<RogueState> NextRogueState(RogueLikeGameData data, RogueState state)
        {
            RogueState newState = RogueState.Init;

            switch (state)
            {
                case RogueState.Init:
                    {
                        newState = RogueState.InArk;
                    }
                    break;
                case RogueState.InArk:
                    {
                        // 玩家选择第一章的地区（从3个1-2★的地区里选一个）
                        OshimaRegion? region = await ChooseRegion(data, 1, 3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅰ：启动 --");
                            data.CurrentRegion = region;
                            data.Chapter1Region = region;
                            data.Region1Map = new RogueLikeMap();
                            data.Region1Map.Load();
                            data.CurrentMap = data.Region1Map;
                            SetupGrid(data, data.CurrentMap);
                            newState = RogueState.Chapter1InArk;
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                        }
                    }
                    break;
                case RogueState.Chapter1InArk:
                    {
                        WriteLine("探索前的准备。");
                        await RestInArk(data);
                        newState = RogueState.ExploringChapter1;
                    }
                    break;
                case RogueState.Chapter2InArk:
                    {
                        WriteLine("回到方舟后，我们得到了新的任务。");
                        // 玩家选择第二章的地区（从3-4★的地区里随机抽取3个来选一个）
                        OshimaRegion? region = await ChooseRegion(data, 2, 3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅱ：启动 --");
                            data.CurrentRegion = region;
                            data.Chapter2Region = region;
                            data.Region2Map = new RogueLikeMap();
                            data.Region2Map.Load();
                            data.CurrentMap = data.Region2Map;
                            SetupGrid(data, data.CurrentMap);
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                            WriteLine("结算到目前为止的奖励。");
                            break;
                        }
                        await RestInArk(data);
                        newState = RogueState.ExploringChapter2;
                    }
                    break;
                case RogueState.Chapter3InArk:
                    {
                        // 玩家选择第三章的地区（从3个5★的地区里选一个）
                        OshimaRegion? region = await ChooseRegion(data, 3, 3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅲ：启动 --");
                            data.CurrentRegion = region;
                            data.Chapter3Region = region;
                            data.Region3Map = new RogueLikeMap();
                            data.Region3Map.Load();
                            data.CurrentMap = data.Region3Map;
                            SetupGrid(data, data.CurrentMap);
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                            WriteLine("结算到目前为止的奖励。");
                            break;
                        }
                        await RestInArk(data);
                        newState = RogueState.ExploringChapter3;
                    }
                    break;
                case RogueState.FinalInArk:
                    WriteLine("当你乘坐返回舱回到方舟时，柔哥没有来迎接你，环视一周，你没有看到任何探员。");
                    WriteLine("在你走出返回舱的那一刹那，你察觉空气中有一股杀气——你很快意识到，方舟遭到了入侵。");
                    WriteLine("-- 【永恒方舟计划】紧急事件·夺还：启动 --");
                    newState = RogueState.ExploringArk;
                    break;
                case RogueState.ExploringChapter1:
                    newState = await ExploreRegion(data, 1);
                    break;
                case RogueState.ExploringChapter2:
                    newState = await ExploreRegion(data, 2);
                    break;
                case RogueState.ExploringChapter3:
                    newState = await ExploreRegion(data, 3);
                    break;
                case RogueState.ArriveChapter1RallyPoint:
                    newState = await ArriveRallyPoint(data, 1);
                    break;
                case RogueState.ArriveChapter2RallyPoint:
                    newState = await ArriveRallyPoint(data, 2);
                    break;
                case RogueState.ArriveChapter3RallyPoint:
                    newState = await ArriveRallyPoint(data, 3);
                    break;
                case RogueState.ExploringArk:
                    await ExploreArk(data);
                    newState = RogueState.FinalBossBattle;
                    break;
                case RogueState.Chapter1BossBattle:
                    {
                        WriteLine("剧情过后，战斗一触即发。");
                        // TODO:调用FunGameActionQueue的战斗系统
                        WriteLine("战胜了BOSS，返回方舟汇报工作，并进行战后整备！");
                        newState = RogueState.Chapter2InArk;
                    }
                    break;
                case RogueState.Chapter2BossBattle:
                    {
                        WriteLine("剧情过后，战斗一触即发。");
                        // TODO:调用FunGameActionQueue的战斗系统
                        WriteLine("战胜了BOSS，返回方舟汇报工作，并进行战后整备！");
                        newState = RogueState.Chapter3InArk;
                    }
                    break;
                case RogueState.Chapter3BossBattle:
                    {
                        WriteLine("剧情过后，战斗一触即发。");
                        // TODO:调用FunGameActionQueue的战斗系统
                        WriteLine("战胜了BOSS，返回方舟汇报工作，并进行战后整备！");
                        newState = RogueState.FinalInArk;
                    }
                    break;
                case RogueState.FinalBossBattle:
                    {
                        WriteLine("剧情过后，战斗一触即发。");
                        // TODO:调用FunGameActionQueue的战斗系统
                        WriteLine("战胜了BOSS，永恒方舟成功夺还！");
                        WriteLine("本次【永恒方舟计划】成功！");
                        newState = RogueState.Finish;
                    }
                    break;
                case RogueState.Finish:
                    break;
                default:
                    break;
            }

            return newState;
        }

        private async Task<OshimaRegion?> ChooseRegion(RogueLikeGameData data, int chapter, int candidate)
        {
            string topic = chapter switch
            {
                1 => "选择初始探索地区",
                _ => "选择下一个探索地区",
            };
            Func<OshimaRegion, bool> predicate = chapter switch
            {
                2 => r => (int)r.Difficulty >= (int)RarityType.ThreeStar && (int)r.Difficulty <= (int)RarityType.FourStar,
                3 => r => (int)r.Difficulty == (int)RarityType.FiveStar,
                _ => r => (int)r.Difficulty <= (int)RarityType.TwoStar
            };
            InquiryOptions options = new(InquiryType.Choice, topic)
            {
                Choices = FunGameConstant.Regions.Where(predicate).OrderBy(r => data.Random.Next()).Take(candidate).ToDictionary(r => r.Name, r => r.ToString())
            };
            InquiryResponse response = await Dispatcher.GetInquiryResponse(options);
            if (!response.Cancel)
            {
                if (response.Choices.FirstOrDefault() is string regionName)
                {
                    OshimaRegion? region = FunGameConstant.Regions.FirstOrDefault(r => r.Name == regionName);
                    if (region != null && region.Areas.Count > 0)
                    {
                        return region;
                    }
                }
            }
            return null;
        }

        private async Task<RogueState> ExploreRegion(RogueLikeGameData data, int chapter)
        {
            RogueState newState = RogueState.Finish;
            if (data.CurrentRegion is null)
            {
                WriteLine("突发！小队和方舟失联了！永恒方舟计划终止。");
                return newState;
            }
            if (data.CurrentMap is null)
            {
                WriteLine($"降落舱在{data.CurrentRegion.Name}的天空中迷失了，你从此永远地消失。永恒方舟计划终止。");
                return newState;
            }
            newState = chapter switch
            {
                1 => RogueState.ArriveChapter1RallyPoint,
                2 => RogueState.ArriveChapter2RallyPoint,
                3 => RogueState.ArriveChapter3RallyPoint,
                _ => RogueState.Finish,
            };

            WriteLine("正在降落...");
            data.CurrentArea = data.CurrentRegion.Areas.OrderByDescending(o => data.Random.Next()).First();
            WriteLine($"在【{data.CurrentRegion.Name}】的【{data.CurrentArea}】区域完成降落！");
            List<Quest> quests = GetQuests(data, data.CurrentRegion, 2);
            data.CurrentQuests = quests;
            AddDialog("柔哥", $"我看到您安全着陆了，现在方舟给您下发任务指令：\r\n{string.Join("\r\n", quests.Select(q => q.ToString()))}");
            Character character = data.Character;

            bool fin = false;
            while (!fin)
            {
                Grid currentGrid = Grid.Empty;

                DisplayMapInConsole(data.CurrentMap);
                currentGrid = data.CurrentMap.GetCharacterCurrentGrid(character) ?? Grid.Empty;

                // 获取可移动范围（曼哈顿距离1格）
                List<Grid> movableGrids = data.CurrentMap.GetGridsByRange(currentGrid, 1, false);

                // 构建选择字典
                Dictionary<string, string> choices = [];
                Dictionary<string, Grid> keyToGrid = [];
                int index = 1;
                foreach (Grid grid in movableGrids.OrderBy(g => g.Y).ThenBy(g => g.X))
                {
                    string dir = GetDirectionName(currentGrid, grid);
                    string roomType = GetRoomTypeDisplay(grid);
                    choices[dir] = $"{roomType} ({grid.X},{grid.Y})";
                    keyToGrid[dir] = grid;
                    index++;
                }

                choices["查看角色状态"] = "";
                if (data.CurrentQuests.All(q => q.Status == QuestState.Completed))
                {
                    choices["前往集结点"] = "结束本地区的探索，并前往本地区的BOSS房间";
                }
                choices["结束本次区域探索"] = "放弃本地区的探索进度并返回方舟";

                InquiryOptions options = new(InquiryType.Choice, "请选择你的下一步行动：")
                {
                    Choices = choices
                };

                InquiryResponse response = await Dispatcher.GetInquiryResponse(options);
                if (response.Cancel)
                {
                    WriteLine("操作已取消。");
                    continue;
                }

                string choice = response.Choices.FirstOrDefault() ?? "";

                if (choice == "查看角色状态")
                {
                    WriteLine(character.GetInfo());
                    continue;
                }

                if (choice == "前往集结点")
                {
                    fin = true;
                    WriteLine("任务目标已完成！你决定前往集结点。");
                    continue;
                }

                if (choice == "结束本次区域探索")
                {
                    fin = true;
                    WriteLine("你决定结束本次探索，返回方舟。");
                    newState = chapter switch
                    {
                        1 => RogueState.InArk,
                        2 => RogueState.Chapter2InArk,
                        3 => RogueState.Chapter3InArk,
                        _ => RogueState.Finish,
                    };
                    continue;
                }

                // 处理移动
                if (keyToGrid.TryGetValue(choice, out Grid? targetGrid) && targetGrid != null)
                {
                    WriteLine($"你移动到了 ({targetGrid.X}, {targetGrid.Y})");
                    data.CurrentMap.CharacterMove(character, currentGrid, targetGrid);
                }
                else
                {
                    WriteLine("无效选择，请重试。");
                }

                CheckQuestProgress(data);
                await Task.Delay(80);
            }
            return newState;
        }

        private void CheckQuestProgress(RogueLikeGameData data)
        {
            WriteLine(string.Join("\r\n", data.CurrentQuests.Select(q => q.ToString())));
        }

        private static string GetDirectionName(Grid current, Grid target)
        {
            int dx = target.X - current.X;
            int dy = target.Y - current.Y;
            return (dx, dy) switch
            {
                (0, -1) => "↑ 北",
                (0, 1) => "↓ 南",
                (-1, 0) => "← 西",
                (1, 0) => "→ 东",
                (-1, -1) => "↖ 西北",
                (1, -1) => "↗ 东北",
                (-1, 1) => "↙ 西南",
                (1, 1) => "↘ 东南",
                _ => "？"
            };
        }
        private static string GetRoomTypeDisplay(Grid grid)
        {
            if (grid.InteractionPoints.Count == 0) return "<空>";
            InteractionPointType type = (InteractionPointType)grid.InteractionPoints.First().CustomValue;
            return type switch
            {
                InteractionPointType.General => "<普>",
                InteractionPointType.Elite => "<精>",
                InteractionPointType.Store => "<商>",
                InteractionPointType.Treasure => "<宝>",
                InteractionPointType.Rest => "<休>",
                InteractionPointType.Change => "<事>",
                _ => "<？>"
            };
        }

        private static List<Quest> GetQuests(RogueLikeGameData data, OshimaRegion region, int count)
        {
            List<Quest> quests = [];

            int immediateQuestCount = data.Random.Next(count + 1);
            int progressiveQuestCount = count - immediateQuestCount;

            var list = region.ImmediateQuestList.OrderBy(kv => data.Random.Next()).Take(immediateQuestCount);
            foreach (var item in list)
            {
                string name = item.Key;
                QuestExploration exploration = item.Value;
                int difficulty = Random.Shared.Next(3, 11);
                Quest quest = new()
                {
                    Id = quests.Count + 1,
                    Name = name,
                    Description = exploration.Description,
                    RegionId = region.Id,
                    NeedyExploreItemName = exploration.Item,
                    QuestType = QuestType.Immediate,
                    Status = QuestState.InProgress
                };
                quests.Add(quest);
            }
            list = region.ProgressiveQuestList.OrderBy(kv => data.Random.Next()).Take(progressiveQuestCount);
            foreach (var item in list)
            {
                string name = item.Key;
                QuestExploration exploration = item.Value;
                int maxProgress = Random.Shared.Next(3, 11);
                Quest quest = new()
                {
                    Id = quests.Count + 1,
                    Name = name,
                    Description = string.Format(exploration.Description, maxProgress),
                    RegionId = region.Id,
                    NeedyExploreItemName = exploration.Item,
                    QuestType = QuestType.Progressive,
                    Progress = 0,
                    MaxProgress = maxProgress,
                    Status = QuestState.InProgress
                };
                quests.Add(quest);
            }

            return quests;
        }

        private async Task<RogueState> ArriveRallyPoint(RogueLikeGameData data, int chapter)
        {
            WriteLine("BOSS房间出现了！做好准备再继续出发吧。");
            bool fin = false;
            while (!fin)
            {
                // TODO:BOSS房前的准备，提供菜单
                fin = true;
            }
            WriteLine("出发！");
            RogueState newState = chapter switch
            {
                1 => RogueState.Chapter1BossBattle,
                2 => RogueState.Chapter2BossBattle,
                3 => RogueState.Chapter3BossBattle,
                _ => RogueState.Finish,
            };
            if (newState == RogueState.Finish)
            {
                WriteLine("你误入了神秘地带，与方舟失联，游戏结束。");
                return newState;
            }
            return newState;
        }

        private async Task RestInArk(RogueLikeGameData data)
        {
            bool fin = false;
            while (!fin)
            {
                // TODO:战后整备，提供菜单回复和提升等
                fin = true;
            }
            WriteLine("出发！");
        }

        private async Task ExploreArk(RogueLikeGameData data)
        {
            data.ArkMap = new RogueLikeMap();
            data.ArkMap.Load();
            data.CurrentMap = data.ArkMap;
            SetupGrid(data, data.CurrentMap);
            bool fin = false;
            while (!fin)
            {
                if (data.CurrentMap != null)
                {
                    DisplayMapInConsole(data.CurrentMap);
                }
                // TODO:方舟事变，需进行方舟探索和收复功能房间并找到最终BOSS房间
                fin = true;
            }
            WriteLine("出发！");
        }

        private void DisplayMapInConsole(GameMap map)
        {
            StringBuilder sb = new();

            // 打印列坐标
            sb.Append("   ");
            for (int x = 0; x < map.Length; x++)
            {
                sb.Append($" {x}\t");
            }

            for (int y = 0; y < map.Width; y++)
            {
                // 打印行坐标
                sb.AppendLine();
                sb.Append($"{y}  ");

                for (int x = 0; x < map.Length; x++)
                {
                    Grid? grid = map[x, y, 0];

                    if (grid is null)
                    {
                        sb.Append("    ");
                        continue;
                    }

                    // 检查格子上是否有角色
                    if (grid.Characters.Count > 0)
                    {
                        // 取第一个角色首字母
                        Character character = grid.Characters.First();
                        string displayChar = character.Name.Length > 0 ? character.Name[0].ToString().ToUpper() : "?";
                        sb.Append($"[{displayChar}] ");
                    }
                    else if (grid.InteractionPoints.Count > 0)
                    {
                        string displayChar = GetRoomTypeDisplay(grid);
                        sb.Append(displayChar);
                    }
                    else
                    {
                        sb.Append(" .  ");
                    }
                    if (x != map.Length - 1) sb.Append('\t');
                }
            }

            WriteLine(sb.ToString());
        }

        private void SetupGrid(RogueLikeGameData data, GameMap map)
        {
            // 生成交互点，8*8的地图，生成30个交互点
            int count = 30;
            Random random = data.Random ?? new();
            Grid[] grids = [.. map.Grids.Values.OrderBy(o => random.Next()).Take(count)];
            foreach (Grid grid in grids)
            {
                // 这些交互点就是房间
                InteractionPoint ip = new()
                {
                    Id = grid.Id,
                    CustomValue = random.Next((int)InteractionPointType.MaxValueMark)
                };
                grid.InteractionPoints.Add(ip);
                grid.CharacterEntered += (character) =>
                {
                    HandleCharacterEnteredGrid(data, character, grid, ip);
                };
            }
            Grid? land = map.Grids.Values.OrderBy(g => random.Next()).FirstOrDefault(g => g.InteractionPoints.Count == 0);
            if (land != null)
            {
                map.SetCharacterCurrentGrid(data.Character, land);
            }
        }

        public void HandleCharacterEnteredGrid(RogueLikeGameData data, Character character, Grid grid, InteractionPoint ip)
        {
            InteractionPointType type = (InteractionPointType)ip.CustomValue;
            // 移除交互点，战斗、宝箱、事件、休息点不会再触发，商店会一直存在
            if (type != InteractionPointType.Store)
            {
                if (!grid.InteractionPoints.Remove(ip))
                {
                    return;
                }
            }

            switch (type)
            {
                case InteractionPointType.General:
                    SyncAwaiter.Wait(HandleGeneralBattle(data, character));
                    break;
                case InteractionPointType.Elite:
                    SyncAwaiter.Wait(HandleEliteBattle(data, character));
                    break;
                case InteractionPointType.Store:
                    SyncAwaiter.Wait(HandleStore(data, character));
                    break;
                case InteractionPointType.Treasure:
                    SyncAwaiter.Wait(HandleTreasure(data, character));
                    break;
                case InteractionPointType.Rest:
                    HandleRest(character);
                    break;
                case InteractionPointType.Change:
                    SyncAwaiter.Wait(HandleRandomEvent(data, character));
                    break;
                default:
                    WriteLine("你踏入了一片未知的区域，但什么也没有发生。");
                    break;
            }
        }

        private async Task HandleGeneralBattle(RogueLikeGameData data, Character character)
        {
            WriteLine("⚔️ 遭遇战！");
            WriteLine("敌人出现了，战斗一触即发！");

            // 模拟战斗流程
            bool victory = await SimulateBattle(data, character, isElite: false);

            if (victory)
            {
                int creditsReward = data.Random.Next(10, 21);
                int materialsReward = data.Random.Next(1, 4);
                character.User.Inventory.Credits += creditsReward;
                character.User.Inventory.Materials += materialsReward;
                WriteLine($"战斗胜利！获得了 {creditsReward:0.##} {Credit_Name} 和 {materialsReward:0.##} {Material_Name}。");

                // 有概率掉落物品（可扩展）
                if (data.Random.NextDouble() < 0.2)
                {
                    WriteLine("敌人掉落了【小型医疗包】！");
                    // 实际物品添加逻辑
                }

                // 更新探索任务进度
                UpdateQuestProgress(data, "战斗");
            }
            else
            {
                WriteLine("战斗失败，你被迫撤退。");
                character.HP = (int)(character.MaxHP * 0.3);
                character.MP = (int)(character.MaxMP * 0.3);
                WriteLine($"你的状态变得很糟糕...\n{character.GetInfo()}");
            }
        }

        private async Task HandleEliteBattle(RogueLikeGameData data, Character character)
        {
            WriteLine("👾 精英战斗！");
            WriteLine("一股强大的气息扑面而来，精英敌人出现了！");

            bool victory = await SimulateBattle(data, character, isElite: true);

            if (victory)
            {
                int creditsReward = data.Random.Next(30, 51);
                int materialsReward = data.Random.Next(3, 7);
                character.User.Inventory.Credits += creditsReward;
                character.User.Inventory.Materials += materialsReward;
                WriteLine($"苦战后获胜！获得了 {creditsReward:0.##} {Credit_Name} 和 {materialsReward:0.##} {Material_Name}。");

                // 精英战必定掉落一个遗物/装备（模拟）
                WriteLine("精英敌人掉落了一件【稀有装备】！");

                UpdateQuestProgress(data, "精英战斗");
            }
            else
            {
                WriteLine("你倒在了精英敌人的脚下，被紧急传送回方舟。");
                character.HP = 1;
                character.MP = 0;
                WriteLine($"重伤状态...\n{character.GetInfo()}");
                // 可以标记本次探索失败或返回方舟
            }
        }

        private async Task HandleStore(RogueLikeGameData data, Character character)
        {
            WriteLine("🏪 你发现了一台自动售货机（或者一位流浪商人）。");
            WriteLine($"当前持有：{character.User.Inventory.Credits:0.##} {Credit_Name}，{character.User.Inventory.Materials:0.##} {Material_Name}。");

            // 构建商店选项
            Dictionary<string, string> storeItems = new()
            {
                ["购买医疗包"] = $"恢复30%生命值 (15 {Credit_Name})",
                ["购买能量饮料"] = $"恢复30%魔法值 (10 {Credit_Name})",
                ["购买武器升级"] = $"永久提升5%攻击力 (30 {Credit_Name})",
                ["购买防御芯片"] = $"永久提升5%防御力 (30 {Credit_Name})",
                ["离开商店"] = ""
            };

            bool shopping = true;
            while (shopping)
            {
                InquiryOptions options = new(InquiryType.Choice, "请选择你要购买的商品：")
                {
                    Choices = storeItems
                };

                InquiryResponse response = await Dispatcher.GetInquiryResponse(options);
                if (response.Cancel || response.Choices.FirstOrDefault() == "离开商店")
                {
                    shopping = false;
                    WriteLine("你离开了商店。");
                    continue;
                }

                string choice = response.Choices.FirstOrDefault() ?? "";
                bool purchased = false;

                switch (choice)
                {
                    case "购买医疗包":
                        if (character.User.Inventory.Credits >= 15)
                        {
                            character.User.Inventory.Credits -= 15;
                            int heal = (int)(character.MaxHP * 0.3);
                            character.HP += heal;
                            WriteLine($"使用了医疗包，恢复了 {heal:0.##} 点生命值。");
                            purchased = true;
                        }
                        else WriteLine($"{Credit_Name}不足！");
                        break;

                    case "购买能量饮料":
                        if (character.User.Inventory.Credits >= 10)
                        {
                            character.User.Inventory.Credits -= 10;
                            int recover = (int)(character.MaxMP * 0.3);
                            character.MP += recover;
                            WriteLine($"饮用了能量饮料，恢复了 {recover:0.##} 点魔法值。");
                            purchased = true;
                        }
                        else WriteLine($"{Credit_Name}不足！");
                        break;

                    case "购买武器升级":
                        if (character.User.Inventory.Credits >= 30)
                        {
                            character.User.Inventory.Credits -= 30;
                            character.ExATKPercentage += 0.05;
                            WriteLine($"武器升级完成！攻击力提升至 {character.ATK:0.##}。");
                            purchased = true;
                            storeItems.Remove(choice); // 限购一次
                        }
                        else WriteLine($"{Credit_Name}不足！");
                        break;

                    case "购买防御芯片":
                        if (character.User.Inventory.Credits >= 30)
                        {
                            character.User.Inventory.Credits -= 30;
                            character.ExDEFPercentage += 0.05;
                            WriteLine($"防御芯片安装完成！防御力提升至 {character.DEF:0.##}。");
                            purchased = true;
                            storeItems.Remove(choice);
                        }
                        else WriteLine($"{Credit_Name}不足！");
                        break;
                }

                if (purchased)
                {
                    WriteLine($"剩余{Credit_Name}：{character.User.Inventory.Credits:0.##}");
                }
                await Task.Delay(100);
            }

            UpdateQuestProgress(data, "商店");
        }

        private async Task HandleTreasure(RogueLikeGameData data, Character character)
        {
            WriteLine("🎁 你发现了一个上锁的宝箱！");

            InquiryOptions options = new(InquiryType.Choice, "要尝试打开它吗？")
            {
                Choices = new Dictionary<string, string>
                {
                    ["打开宝箱"] = "可能获得稀有物品，也可能触发陷阱",
                    ["无视它"] = "安全第一"
                }
            };

            InquiryResponse response = await Dispatcher.GetInquiryResponse(options);
            if (response.Cancel || response.Choices.FirstOrDefault() == "无视它")
            {
                WriteLine("你谨慎地绕过了宝箱。");
                return;
            }

            // 开箱结果
            int roll = data.Random.Next(100);
            if (roll < 60) // 60% 正面奖励
            {
                int credits = data.Random.Next(20, 41);
                int materials = data.Random.Next(2, 6);
                character.User.Inventory.Credits += credits;
                character.User.Inventory.Materials += materials;
                WriteLine($"宝箱里装满了物资！获得了 {credits:0.##} {Credit_Name} 和 {materials:0.##} {Material_Name}。");
            }
            else if (roll < 80) // 20% 稀有物品
            {
                WriteLine("宝箱中散发着奇异的光芒... 你获得了【神秘遗物】！");
                // 实际添加遗物逻辑
            }
            else // 20% 陷阱
            {
                int damage = (int)(character.MaxHP * 0.2);
                character.HP = Math.Max(1, character.HP - damage);
                WriteLine($"宝箱突然爆炸！你受到了 {damage:0.##} 点伤害。");
            }

            UpdateQuestProgress(data, "宝箱");
        }

        private void HandleRest(Character character)
        {
            WriteLine("🌸 你来到了一片宁静的休息区。");
            character.Recovery();
            WriteLine($"弥漫香气的花海治愈了你。\n{character.GetInfo()}");
            // 可添加额外 buff 或移除负面状态
        }

        private async Task HandleRandomEvent(RogueLikeGameData data, Character character)
        {
            WriteLine("❓ 你踏入了一个奇异的空间，周围的环境开始扭曲...");
            await Task.Delay(500);

            int eventType = data.Random.Next(5);
            switch (eventType)
            {
                case 0:
                    WriteLine("一位神秘的旅者给了你一些补给。");
                    character.User.Inventory.Credits += 15;
                    WriteLine($"{Credit_Name} +15");
                    break;
                case 1:
                    WriteLine("你不小心踩到了陷阱！");
                    int damage = (int)(character.MaxHP * 0.15);
                    character.HP = Math.Max(1, character.HP - damage);
                    WriteLine($"受到了 {damage:0.##} 点伤害。");
                    break;
                case 2:
                    WriteLine("你发现了一具探险者的遗骸，从他的背包中找到了有用的物资。");
                    character.User.Inventory.Materials += 3;
                    WriteLine($"{Material_Name} +3");
                    break;
                case 3:
                    WriteLine("一股神秘的力量笼罩了你，你感觉身体变得更加强壮。");
                    character.ExHP2 += 10;
                    character.HP += 10;
                    WriteLine($"最大生命值提升了10点！\n{character.GetInfo()}");
                    break;
                case 4:
                    if (data.CurrentQuests.FirstOrDefault(q => q.Status != QuestState.Completed) is Quest quest)
                    {
                        if (quest.QuestType == QuestType.Progressive)
                        {
                            quest.Progress = quest.MaxProgress;
                        }
                        quest.Status = QuestState.Completed;
                        WriteLine($"任务{quest.Name}已完成！");
                    }
                    break;
            }

            UpdateQuestProgress(data, "随机事件");
        }

        // 模拟战斗（临时占位，等待接入实际战斗系统）
        private async Task<bool> SimulateBattle(RogueLikeGameData data, Character character, bool isElite)
        {
            // 简单模拟：基于角色属性和随机数决定胜负
            // 实际应调用 FunGameActionQueue 的战斗系统

            double winRate = 0.7; // 基础胜率
            if (isElite) winRate = 0.5;

            // 根据角色属性调整胜率
            double power = (character.ATK * 0.6 + character.DEF * 0.4) / 50.0;
            winRate = Math.Clamp(winRate + (power - 1) * 0.15, 0.1, 0.95);

            bool victory = data.Random.NextDouble() < winRate;

            // 模拟战斗过程文本
            WriteLine("战斗开始！");
            await Task.Delay(300);
            WriteLine($"敌方生命值：{data.Random.Next(30, 80):0.##}");
            await Task.Delay(300);
            WriteLine($"你发起了攻击，造成了 {character.ATK + data.Random.Next(-5, 10):0.##} 点伤害！");
            await Task.Delay(300);

            if (victory)
            {
                WriteLine("敌人倒下了！");
            }
            else
            {
                WriteLine("敌人反击！你的生命值急速下降...");
            }

            return victory;
        }

        // 辅助方法：更新探索任务进度
        private void UpdateQuestProgress(RogueLikeGameData data, string actionType)
        {
            foreach (var quest in data.CurrentQuests.Where(q => q.Status == QuestState.InProgress))
            {
                if (quest.QuestType == QuestType.Progressive)
                {
                    quest.Progress = Math.Min(quest.MaxProgress, quest.Progress + 1);
                    WriteLine($"[任务进度] {quest.Name}: {quest.Progress}/{quest.MaxProgress}");
                    if (quest.Progress >= quest.MaxProgress)
                    {
                        quest.Status = QuestState.Completed;
                        WriteLine($"任务【{quest.Name}】已完成！");
                    }
                }
                // 即时任务可能需要特定条件，此处简化处理
            }
        }
    }

    public class RogueLikeGameData(Character character, int seed = -1)
    {
        public RogueState RogueState { get; set; } = RogueState.Init;
        public Character Character { get; set; } = character;
        public int Chapter { get; set; } = 1;
        public OshimaRegion? CurrentRegion { get; set; } = null;
        public string CurrentArea { get; set; } = "";
        public List<Quest> CurrentQuests { get; set; } = [];
        public int RoomId { get; set; } = -1;
        public OshimaRegion? Chapter1Region { get; set; } = null;
        public OshimaRegion? Chapter2Region { get; set; } = null;
        public OshimaRegion? Chapter3Region { get; set; } = null;
        public int Seed { get; } = -1;
        public Random Random { get; } = seed == -1 ? new() : new(seed);
        public GameMap? CurrentMap { get; set; } = null;
        public GameMap? Region1Map { get; set; } = null;
        public GameMap? Region2Map { get; set; } = null;
        public GameMap? Region3Map { get; set; } = null;
        public GameMap? ArkMap { get; set; } = null;
    }

    public enum RogueState
    {
        Init,
        InArk,
        Chapter1InArk,
        Chapter2InArk,
        Chapter3InArk,
        FinalInArk,
        ExploringChapter1,
        ArriveChapter1RallyPoint,
        ExploringChapter2,
        ArriveChapter2RallyPoint,
        ExploringChapter3,
        ArriveChapter3RallyPoint,
        ExploringArk,
        Chapter1BossBattle,
        Chapter2BossBattle,
        Chapter3BossBattle,
        FinalBossBattle,
        Finish
    }

    public enum InteractionPointType
    {
        /// <summary>
        /// 普通战斗
        /// </summary>
        General,
        /// <summary>
        /// 精英
        /// </summary>
        Elite,
        /// <summary>
        /// 商店
        /// </summary>
        Store,
        /// <summary>
        /// 宝箱
        /// </summary>
        Treasure,
        /// <summary>
        /// 休息点
        /// </summary>
        Rest,
        /// <summary>
        /// 随机事件
        /// </summary>
        Change,
        /// <summary>
        /// 最大值标记，仅用于生成时限定范围
        /// </summary>
        MaxValueMark
    }

    public class RogueLikeMap : GameMap
    {
        public override string Name => "milimoe.fungame.roguelike.map";

        public override string Description => "GameMap for RogueLike";

        public override string Version => "1.0.0";

        public override string Author => "Milimoe";

        public override int Length => 8;

        public override int Width => 8;

        public override int Height => 1;

        public override float Size => 3;

        public RogueLikeGameData? RogueLikeGameData { get; set; } = default;

        public override GameMap InitGamingQueue(IGamingQueue queue)
        {
            return new RogueLikeMap();
        }
    }
}
