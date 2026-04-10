using System.Collections.Concurrent;
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

namespace Milimoe.FunGame.Testing.Solutions
{
    public class RogueLikeServer(RogueLikeDispatcher dispatcher)
    {
        public bool Running => Dispatcher.Running;
        public Task? Guard { get; set; } = null;
        public RogueLikeDispatcher Dispatcher { get; set; } = dispatcher;
        public ConcurrentFIFOQueue<(Guid Guid, DataRequestArgs Args)> Inquiries { get; } = [];
        public ConcurrentDictionary<string, User> Users { get; set; } = [];
        public ConcurrentDictionary<string, RogueLikeGameLoopWorker> RogueLikeGameWorkers { get; set; } = [];

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
                if (!Inquiries.IsEmpty && Inquiries.Dequeue(out var obj))
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
            RogueLikeGameLoopWorker worker = new(this, user);
            RogueLikeGameWorkers[username] = worker;
            await worker.GameLoop();
        }

        public async Task<InquiryResponse> GetInquiryResponse(InquiryOptions options) => await Dispatcher.GetInquiryResponse(options);

        private async Task<DataRequestArgs> HandleDataRequest(DataRequestArgs args)
        {
            DataRequestArgs response = new(args.RequestType);

            switch (response.RequestType)
            {
                case "createuser":
                    {
                        WriteLine("建立一个存档，这是属于你的【永恒方舟】。");
                        string username = "";
                        do
                        {
                            InquiryResponse inquiryResponse = await Dispatcher.GetInquiryResponse(new(InquiryType.TextInput, "为【永恒方舟】命名"));
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
                        WriteLine($"永恒方舟【{username}】号，起航！正在发射至预定轨道……");
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
                            WriteLine($"欢迎回来，这里是永恒方舟【{user.Username}】号。");
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
    }

    public class RogueLikeGameLoopWorker
    {
        public RogueLikeServer Server { get; set; }
        public User User { get; set; }
        public RogueLikeGameData Data { get; set; }
        public string Credit_Name { get; set; } = General.GameplayEquilibriumConstant.InGameCurrency;
        public string Material_Name { get; set; } = General.GameplayEquilibriumConstant.InGameMaterial;

        public RogueLikeGameLoopWorker(RogueLikeServer server, User user)
        {
            Server = server;
            User = user;
            Data = new(User.Inventory.MainCharacter);
        }

        public void WriteLine(string message = "") => Server.WriteLine(message);

        public void AddDialog(string speaker = "", string message = "") => Server.AddDialog(speaker, message);

        public async Task<InquiryResponse> GetInquiryResponse(InquiryOptions options) => await Server.GetInquiryResponse(options);

        public async Task GameLoop()
        {
            while (Data.RogueState != RogueState.Finish)
            {
                Data.RogueState = await NextRogueState(Data.RogueState);
                if (Data.RogueState == RogueState.Init)
                {
                    Data.RogueState = RogueState.Finish;
                }
                await Task.Delay(100);
            }
        }

        private async Task<RogueState> NextRogueState(RogueState state)
        {
            RogueState newState = RogueState.Init;

            switch (state)
            {
                case RogueState.Init:
                    {
                        // 选择角色出发
                        AddDialog("？？？", "您是谁……？");
                        Character? character = await ChooseCharacter();
                        if (character != null)
                        {
                            Data.Character = character;
                            AddDialog("柔哥", $"您好，{character.NickName}探员。我是【柔哥】，作为方舟上的智能 AI，我将协助您前往指定任务地点开展本次勘测工作。");
                            newState = RogueState.InArk;
                        }
                        else
                        {
                            WriteLine("未选择角色，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                        }
                    }
                    break;
                case RogueState.InArk:
                    {
                        // 玩家选择第一章的地区（从3个1-2★的地区里选一个）
                        OshimaRegion? region = await ChooseRegion(3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅰ：启动 --");
                            Data.Chapter = 1;
                            Data.CurrentRegion = region;
                            Data.Chapter1Region = region;
                            Data.Region1Map = new RogueLikeMap();
                            Data.Region1Map.Load();
                            Data.CurrentMap = Data.Region1Map;
                            SetupGrid(Data.CurrentMap);
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
                        await RestInArk();
                        newState = RogueState.ExploringChapter1;
                    }
                    break;
                case RogueState.Chapter2InArk:
                    {
                        WriteLine("回到方舟后，你得到了新的任务。");
                        // 玩家选择第二章的地区（从3-4★的地区里随机抽取3个来选一个）
                        OshimaRegion? region = await ChooseRegion(3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅱ：启动 --");
                            Data.Chapter = 2;
                            Data.CurrentRegion = region;
                            Data.Chapter2Region = region;
                            Data.Region2Map = new RogueLikeMap();
                            Data.Region2Map.Load();
                            Data.CurrentMap = Data.Region2Map;
                            SetupGrid(Data.CurrentMap);
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                            WriteLine("结算到目前为止的奖励。");
                            break;
                        }
                        await RestInArk();
                        newState = RogueState.ExploringChapter2;
                    }
                    break;
                case RogueState.Chapter3InArk:
                    {
                        // 玩家选择第三章的地区（从3个5★的地区里选一个）
                        OshimaRegion? region = await ChooseRegion(3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅲ：启动 --");
                            Data.Chapter = 3;
                            Data.CurrentRegion = region;
                            Data.Chapter3Region = region;
                            Data.Region3Map = new RogueLikeMap();
                            Data.Region3Map.Load();
                            Data.CurrentMap = Data.Region3Map;
                            SetupGrid(Data.CurrentMap);
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                            WriteLine("结算到目前为止的奖励。");
                            break;
                        }
                        await RestInArk();
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
                    newState = await ExploreRegion();
                    break;
                case RogueState.ExploringChapter2:
                    newState = await ExploreRegion();
                    break;
                case RogueState.ExploringChapter3:
                    newState = await ExploreRegion();
                    break;
                case RogueState.ArriveChapter1RallyPoint:
                    newState = await ArriveRallyPoint(1);
                    break;
                case RogueState.ArriveChapter2RallyPoint:
                    newState = await ArriveRallyPoint(2);
                    break;
                case RogueState.ArriveChapter3RallyPoint:
                    newState = await ArriveRallyPoint(3);
                    break;
                case RogueState.ExploringArk:
                    await ExploreArk();
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

        private async Task<Character?> ChooseCharacter()
        {
            List<Character> characters = FunGameConstant.Characters;

            const int pageSize = 6;
            int currentPage = 1;
            int maxPage = characters.MaxPage(pageSize);

            bool choosing = true;
            while (choosing)
            {
                // 获取当前页物品
                Character[] currentPageCharacters = [.. characters.GetPage(currentPage, pageSize)];

                // 构建显示选项
                Dictionary<string, string> chooseChoices = [];

                foreach (Character c in currentPageCharacters)
                {
                    c.Level = 1;
                    chooseChoices[$"选择 {c.NickName}"] = c.GetInfo();
                }

                // 分页控制选项
                if (maxPage > 1)
                {
                    if (currentPage < maxPage)
                        chooseChoices["下一页"] = $"当前 {currentPage} / {maxPage} 页";
                    if (currentPage > 1)
                        chooseChoices["上一页"] = $"当前 {currentPage} / {maxPage} 页";
                }

                chooseChoices["返回主菜单"] = "结束本次游戏";

                InquiryOptions options = new(InquiryType.Choice, $"【选择你的角色】第 {currentPage} / {maxPage} 页")
                {
                    Choices = chooseChoices
                };

                InquiryResponse response = await GetInquiryResponse(options);

                if (response.Cancel || response.Choices.FirstOrDefault() == "返回主菜单")
                {
                    choosing = false;
                    WriteLine("永恒方舟计划终止。");
                    continue;
                }

                string choice = response.Choices.FirstOrDefault() ?? "";

                // 处理翻页
                if (choice == "上一页")
                {
                    if (currentPage > 1) currentPage--;
                    continue;
                }
                if (choice == "下一页")
                {
                    if (currentPage < maxPage) currentPage++;
                    continue;
                }

                // 处理选择
                if (choice.StartsWith("选择 "))
                {
                    string cName = choice[3..]; // 去掉“选择 ”前缀
                    Character? selectedCharacter = characters.FirstOrDefault(c => c.NickName == cName)?.Copy();

                    if (selectedCharacter != null)
                    {
                        return selectedCharacter;
                    }
                    else
                    {
                        WriteLine("未找到该角色，请重试。");
                    }
                }
            }

            return null;
        }

        private async Task<OshimaRegion?> ChooseRegion(int candidate)
        {
            string topic = Data.Chapter switch
            {
                1 => "选择初始探索地区",
                _ => "选择下一个探索地区",
            };
            Func<OshimaRegion, bool> predicate = Data.Chapter switch
            {
                2 => r => (int)r.Difficulty >= (int)RarityType.ThreeStar && (int)r.Difficulty <= (int)RarityType.FourStar,
                3 => r => (int)r.Difficulty == (int)RarityType.FiveStar,
                _ => r => (int)r.Difficulty <= (int)RarityType.TwoStar
            };
            InquiryOptions options = new(InquiryType.Choice, topic)
            {
                Choices = FunGameConstant.Regions.Where(predicate).OrderBy(r => Data.Random.Next()).Take(candidate).ToDictionary(r => r.Name, r => r.ToString())
            };
            InquiryResponse response = await GetInquiryResponse(options);
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

        private async Task<RogueState> ExploreRegion()
        {
            RogueState newState = RogueState.Finish;
            if (Data.CurrentRegion is null)
            {
                WriteLine("突发！小队和方舟失联了！永恒方舟计划终止。");
                return newState;
            }
            if (Data.CurrentMap is null)
            {
                WriteLine($"降落舱在{Data.CurrentRegion.Name}的天空中迷失了，你从此永远地消失。永恒方舟计划终止。");
                return newState;
            }
            newState = Data.Chapter switch
            {
                1 => RogueState.ArriveChapter1RallyPoint,
                2 => RogueState.ArriveChapter2RallyPoint,
                3 => RogueState.ArriveChapter3RallyPoint,
                _ => RogueState.Finish,
            };

            WriteLine("正在降落...");
            Data.CurrentArea = Data.CurrentRegion.Areas.OrderByDescending(o => Data.Random.Next()).First();
            WriteLine($"在【{Data.CurrentRegion.Name}】的【{Data.CurrentArea}】区域完成降落！");
            AddDialog("柔哥", $"我看到您安全着陆了，现在方舟给您下发任务指令：\r\n{string.Join("\r\n", Data.CurrentQuests.Select(q => q.ToString()))}");
            Character character = Data.Character;

            bool fin = false;
            while (!fin)
            {
                Grid currentGrid = Grid.Empty;

                DisplayMapInConsole(Data.CurrentMap);
                currentGrid = Data.CurrentMap.GetCharacterCurrentGrid(character) ?? Grid.Empty;

                // 获取可移动范围
                List<Grid> movableGrids = [.. Data.CurrentMap.GetGridsBySquareRange(currentGrid, 1, false)];

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
                if (Data.CurrentQuests.All(q => q.Status == QuestState.Completed))
                {
                    choices["前往集结点"] = "结束本地区的探索，并前往本地区的BOSS房间";
                }
                choices["结束本次区域探索"] = "放弃本地区的探索进度并返回方舟";

                InquiryOptions options = new(InquiryType.Choice, "请选择你的下一步行动：")
                {
                    Choices = choices
                };

                InquiryResponse response = await GetInquiryResponse(options);
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
                    newState = Data.Chapter switch
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
                    Data.CurrentMap.CharacterMove(character, currentGrid, targetGrid);
                }
                else
                {
                    WriteLine("无效选择，请重试。");
                }

                CheckQuestProgress();
                await Task.Delay(80);
            }
            return newState;
        }

        private void CheckQuestProgress()
        {
            WriteLine(string.Join("\r\n", Data.CurrentQuests.Select(q => q.ToString())));
        }

        private async Task<RogueState> ArriveRallyPoint(int chapter)
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

        private async Task RestInArk()
        {
            Func<Item, bool> predicate = Data.Chapter switch
            {
                2 => i => (int)i.QualityType >= (int)QualityType.Green && (int)i.QualityType <= (int)QualityType.Orange,
                3 => i => (int)i.QualityType >= (int)QualityType.Purple && (int)i.QualityType <= (int)QualityType.Red,
                _ => i => (int)i.QualityType >= (int)QualityType.White && (int)i.QualityType <= (int)QualityType.Blue
            };
            Item[] storeItems = [.. FunGameConstant.PlayerRegions.First().Items.Where(predicate)];
            bool fin = false;
            while (!fin)
            {
                // TODO:战后整备，提供菜单回复和提升等
                // 主菜单
                Dictionary<string, string> mainMenu = new()
                {
                    ["查看角色状态"] = "",
                    ["方舟商店"] = "购买恢复道具、装备、消耗品等",
                    ["提升角色能力"] = "消耗材料永久提升属性",
                    ["出发！"] = "继续下一阶段探索"
                };

                InquiryOptions mainOptions = new(InquiryType.Choice, "【方舟整备中心】请选择操作：")
                {
                    Choices = mainMenu
                };

                InquiryResponse mainResponse = await GetInquiryResponse(mainOptions);

                if (mainResponse.Cancel)
                {
                    WriteLine("操作已取消。");
                    continue;
                }

                string mainChoice = mainResponse.Choices.FirstOrDefault() ?? "";

                switch (mainChoice)
                {
                    case "查看角色状态":
                        WriteLine(Data.Character.GetInfo());
                        break;

                    case "方舟商店":
                        await HandleArkStore(storeItems);
                        break;

                    case "提升角色能力":
                        // TODO: 未来可扩展为属性点分配、技能解锁等系统
                        WriteLine("【能力提升】系统正在开发中，敬请期待...");
                        break;

                    case "出发！":
                        fin = true;
                        WriteLine("整备完毕，出发！");
                        break;

                    default:
                        WriteLine("无效选择，请重试。");
                        break;
                }
            }
            WriteLine("出发！");
        }

        private async Task HandleArkStore(Item[] storeItems)
        {
            if (storeItems.Length == 0)
            {
                WriteLine("当前章节暂无可用商店物品。");
                return;
            }

            const int pageSize = 8;
            int currentPage = 1;
            int maxPage = storeItems.MaxPage(pageSize);

            bool shopping = true;
            while (shopping)
            {
                // 获取当前页物品
                Item[] currentPageItems = [.. storeItems.GetPage(currentPage, pageSize)];

                // 构建显示选项
                Dictionary<string, string> shopChoices = [];

                foreach (Item item in currentPageItems)
                {
                    string priceStr = item.Price > 0 ? $" ({item.Price} {Credit_Name})" : "";
                    string desc = string.IsNullOrEmpty(item.Description) ? "无描述" : item.Description;
                    shopChoices[$"购买 {item.Name}"] = $"{desc}{priceStr} | 品质: {ItemSet.GetQualityTypeName(item.QualityType)}";
                }

                // 分页控制选项
                if (maxPage > 1)
                {
                    if (currentPage < maxPage)
                        shopChoices["下一页"] = $"当前 {currentPage} / {maxPage} 页";
                    if (currentPage > 1)
                        shopChoices["上一页"] = $"当前 {currentPage} / {maxPage} 页";
                }

                shopChoices["返回主菜单"] = "";

                InquiryOptions options = new(InquiryType.Choice,
                    $"【方舟商店】 第 {currentPage} / {maxPage} 页 | 当前持有：{Data.Character.User.Inventory.Credits} {Credit_Name}")
                {
                    Choices = shopChoices
                };

                InquiryResponse response = await GetInquiryResponse(options);

                if (response.Cancel || response.Choices.FirstOrDefault() == "返回主菜单")
                {
                    shopping = false;
                    WriteLine("已离开方舟商店。");
                    continue;
                }

                string choice = response.Choices.FirstOrDefault() ?? "";

                // 处理翻页
                if (choice == "上一页")
                {
                    if (currentPage > 1) currentPage--;
                    continue;
                }
                if (choice == "下一页")
                {
                    if (currentPage < maxPage) currentPage++;
                    continue;
                }

                // 处理购买
                if (choice.StartsWith("购买 "))
                {
                    string itemName = choice[3..]; // 去掉“购买 ”前缀
                    Item? selectedItem = storeItems.FirstOrDefault(i => i.Name == itemName);

                    if (selectedItem != null)
                    {
                        await TryPurchaseItem(selectedItem);
                    }
                    else
                    {
                        WriteLine("未找到该物品，请重试。");
                    }
                }
            }
        }

        private async Task TryPurchaseItem(Item item)
        {
            if (Data.Character.User.Inventory.Credits < item.Price)
            {
                WriteLine($"【购买失败】{Credit_Name} 不足！需要 {item.Price}，当前持有 {Data.Character.User.Inventory.Credits}");
                return;
            }

            // 执行购买
            Data.Character.User.Inventory.Credits -= item.Price;

            WriteLine($"剩余 {Credit_Name}：{Data.Character.User.Inventory.Credits}");

            await Task.Delay(100);
        }

        private async Task ExploreArk()
        {
            Data.Chapter = 4;
            Data.ArkMap = new RogueLikeMap();
            Data.ArkMap.Load();
            Data.CurrentMap = Data.ArkMap;
            SetupGrid(Data.CurrentMap);
            bool fin = false;
            while (!fin)
            {
                if (Data.CurrentMap != null)
                {
                    DisplayMapInConsole(Data.CurrentMap);
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
                        string displayChar = character.NickName.Length > 0 ? character.NickName[0].ToString().ToUpper() : "?";
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

        private void SetupGrid(GameMap map)
        {
            Random random = Data.Random;

            // ====================== 第一步：先生成任务 ======================
            List<Quest> quests = GetQuests(Data.CurrentRegion!, 2);  // 保持原生成数量，可后续调整
            Data.CurrentQuests = quests;

            // ====================== 第二步：根据任务需求生成房间 ======================
            // 房间类型最小/最大生成数量配置
            var roomConfig = new Dictionary<InteractionPointType, (int min, int max)>
            {
                { InteractionPointType.General, (5, -1) },   // 普通战斗 - 无上限
                { InteractionPointType.Elite,   (4, -1) },   // 精英 - 无上限
                { InteractionPointType.Store,   (2, 2) },    // 商店 - 固定2个
                { InteractionPointType.Treasure,(3, 6) },    // 宝箱
                { InteractionPointType.Rest,    (2, 5) },    // 休息
                { InteractionPointType.Change,  (3, -1) }    // 随机事件 - 无上限
            };

            // 统计任务需要的房间类型
            Dictionary<InteractionPointType, int> requiredRooms = [];

            // 即时任务：确保至少有一个对应房间
            foreach (Quest quest in quests.Where(q => q.QuestType == QuestType.Immediate))
            {
                InteractionPointType neededType = GetNeededRoomTypeForQuest(quest);
                if (!requiredRooms.ContainsKey(neededType))
                    requiredRooms[neededType] = 0;
                requiredRooms[neededType] = Math.Max(requiredRooms[neededType], 1);
            }

            // 渐进任务：确保有足够“行动次数”（溢出）
            int totalActionsNeeded = quests
                .Where(q => q.QuestType == QuestType.Progressive)
                .Sum(q => q.MaxProgress);

            requiredRooms[InteractionPointType.General] = Math.Max(
                requiredRooms.GetValueOrDefault(InteractionPointType.General, 0),
                (int)(totalActionsNeeded * 0.55) + 3);

            requiredRooms[InteractionPointType.Elite] = Math.Max(
                requiredRooms.GetValueOrDefault(InteractionPointType.Elite, 0),
                (int)(totalActionsNeeded * 0.15) + 1);

            requiredRooms[InteractionPointType.Change] = Math.Max(
                requiredRooms.GetValueOrDefault(InteractionPointType.Change, 0),
                (int)(totalActionsNeeded * 0.20) + 2);

            // ====================== 实际生成房间 ======================
            List<Grid> allGrids = [.. map.Grids.Values];
            List<Grid> selectedGrids = [.. allGrids.OrderBy(_ => random.Next())];

            int gridIndex = 0;
            Dictionary<InteractionPointType, int> currentCounts = [];

            // 1. 先强制生成最小数量（包含任务需求）
            foreach (var (type, (min, _)) in roomConfig)
            {
                int actualMin = Math.Max(min, requiredRooms.GetValueOrDefault(type, 0));
                for (int i = 0; i < actualMin && gridIndex < selectedGrids.Count; i++)
                {
                    Grid grid = selectedGrids[gridIndex++];
                    AddInteractionPoint(grid, type);
                    currentCounts[type] = currentCounts.GetValueOrDefault(type, 0) + 1;
                }
            }

            // 2. 动态补充剩余房间，直到总数达到30个
            while (gridIndex < 30 && gridIndex < selectedGrids.Count)
            {
                InteractionPointType type = ChooseRoomTypeByWeight(roomConfig, currentCounts);

                // 检查是否超过最大限制
                int max = roomConfig[type].max;
                if (max != -1 && currentCounts.GetValueOrDefault(type, 0) >= max)
                {
                    // 如果该类型已满，尝试其他类型（避免死循环）
                    if (roomConfig.All(kv => kv.Value.max != -1 && currentCounts.GetValueOrDefault(kv.Key, 0) >= kv.Value.max))
                        break;
                    continue;
                }

                Grid grid = selectedGrids[gridIndex++];
                AddInteractionPoint(grid, type);
                currentCounts[type] = currentCounts.GetValueOrDefault(type, 0) + 1;
            }

            // 如果实际生成的少于30个，用普通或精英战斗、事件补满
            while (gridIndex < 30 && gridIndex < selectedGrids.Count)
            {
                Grid grid = selectedGrids[gridIndex++];
                InteractionPointType type = Data.Chapter switch // 根据当前章节决定补全策略
                {
                    1 => random.Next(3) switch
                    {
                        0 => InteractionPointType.Elite,
                        1 => InteractionPointType.Change,
                        _ => InteractionPointType.General
                    },
                    2 => random.Next(10) switch
                    {
                        0 or 1 or 2 or 3 => InteractionPointType.Elite,   // 40%
                        4 or 5 or 6 => InteractionPointType.Change,  // 30%
                        _ => InteractionPointType.General  // 30%
                    },
                    3 => InteractionPointType.Elite,
                    _ => InteractionPointType.General,
                };
                AddInteractionPoint(grid, type);
                currentCounts[type] = currentCounts.GetValueOrDefault(type, 0) + 1;
            }

            // 确保起始位置为空地（无交互点）
            Grid? land = allGrids.OrderBy(g => random.Next()).FirstOrDefault(g => g.InteractionPoints.Count == 0);
            if (land != null)
            {
                map.SetCharacterCurrentGrid(Data.Character, land);
            }

            // 输出任务和房间生成信息（调试用）
            WriteLine($"本区域任务已生成：{quests.Count} 个");
            WriteLine($"房间生成完成（总计 {gridIndex} 个）");
            WriteLine($"房间分布：普通战 {CountRoomType(map, InteractionPointType.General)} | 精英战 {CountRoomType(map, InteractionPointType.Elite)} | 商店 {CountRoomType(map, InteractionPointType.Store)} | 宝箱 {CountRoomType(map, InteractionPointType.Treasure)} | 休息点 {CountRoomType(map, InteractionPointType.Rest)} | 事件 {CountRoomType(map, InteractionPointType.Change)}");
        }

        public void HandleCharacterEnteredGrid(Character character, Grid grid, InteractionPoint ip)
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
                    SyncAwaiter.Wait(HandleGeneralBattle(character));
                    break;
                case InteractionPointType.Elite:
                    SyncAwaiter.Wait(HandleEliteBattle(character));
                    break;
                case InteractionPointType.Store:
                    SyncAwaiter.Wait(HandleStore(character));
                    break;
                case InteractionPointType.Treasure:
                    SyncAwaiter.Wait(HandleTreasure(character));
                    break;
                case InteractionPointType.Rest:
                    HandleRest(character);
                    break;
                case InteractionPointType.Change:
                    SyncAwaiter.Wait(HandleRandomEvent(character));
                    break;
                default:
                    WriteLine("你踏入了一片未知的区域，但什么也没有发生。");
                    break;
            }
        }

        private async Task HandleGeneralBattle(Character character)
        {
            WriteLine("⚔️ 遭遇战！");
            WriteLine("敌人出现了，战斗一触即发！");

            // 模拟战斗流程
            bool victory = await SimulateBattle(character, isElite: false);

            if (victory)
            {
                int creditsReward = Data.Random.Next(10, 21);
                int materialsReward = Data.Random.Next(1, 4);
                character.User.Inventory.Credits += creditsReward;
                character.User.Inventory.Materials += materialsReward;
                WriteLine($"战斗胜利！获得了 {creditsReward:0.##} {Credit_Name} 和 {materialsReward:0.##} {Material_Name}。");

                // 有概率掉落物品（可扩展）
                if (Data.Random.NextDouble() < 0.2)
                {
                    WriteLine("敌人掉落了【小型医疗包】！");
                    // 实际物品添加逻辑
                }

                // 更新探索任务进度
                UpdateQuestProgress("战斗");
            }
            else
            {
                WriteLine("战斗失败，你被迫撤退。");
                character.HP = (int)(character.MaxHP * 0.3);
                character.MP = (int)(character.MaxMP * 0.3);
                WriteLine($"你的状态变得很糟糕...\n{character.GetInfo()}");
            }
        }

        private async Task HandleEliteBattle(Character character)
        {
            WriteLine("👾 精英战斗！");
            WriteLine("一股强大的气息扑面而来，精英敌人出现了！");

            bool victory = await SimulateBattle(character, isElite: true);

            if (victory)
            {
                int creditsReward = Data.Random.Next(30, 51);
                int materialsReward = Data.Random.Next(3, 7);
                character.User.Inventory.Credits += creditsReward;
                character.User.Inventory.Materials += materialsReward;
                WriteLine($"苦战后获胜！获得了 {creditsReward:0.##} {Credit_Name} 和 {materialsReward:0.##} {Material_Name}。");

                // 精英战必定掉落一个遗物/装备（模拟）
                WriteLine("精英敌人掉落了一件【稀有装备】！");

                UpdateQuestProgress("精英战斗");
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

        private async Task HandleStore(Character character)
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

                InquiryResponse response = await GetInquiryResponse(options);
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

            UpdateQuestProgress("商店");
        }

        private async Task HandleTreasure(Character character)
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

            InquiryResponse response = await GetInquiryResponse(options);
            if (response.Cancel || response.Choices.FirstOrDefault() == "无视它")
            {
                WriteLine("你谨慎地绕过了宝箱。");
                return;
            }

            // 开箱结果
            int roll = Data.Random.Next(100);
            if (roll < 60) // 60% 正面奖励
            {
                int credits = Data.Random.Next(20, 41);
                int materials = Data.Random.Next(2, 6);
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

            UpdateQuestProgress("宝箱");
        }

        private void HandleRest(Character character)
        {
            WriteLine("🌸 你来到了一片宁静的休息区。");
            character.Recovery();
            WriteLine($"弥漫香气的花海治愈了你。\n{character.GetInfo()}");
            // 可添加额外 buff 或移除负面状态
        }

        private async Task HandleRandomEvent(Character character)
        {
            WriteLine("❓ 你踏入了一个奇异的空间，周围的环境开始扭曲...");
            await Task.Delay(500);

            int eventType = Data.Random.Next(5);
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
                    if (Data.CurrentQuests.FirstOrDefault(q => q.Status != QuestState.Completed) is Quest quest)
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

            UpdateQuestProgress("随机事件");
        }

        // 模拟战斗（临时占位，等待接入实际战斗系统）
        private async Task<bool> SimulateBattle(Character character, bool isElite)
        {
            // 简单模拟：基于角色属性和随机数决定胜负
            // 实际应调用 FunGameActionQueue 的战斗系统

            double winRate = 0.7; // 基础胜率
            if (isElite) winRate = 0.5;

            // 根据角色属性调整胜率
            double power = (character.ATK * 0.6 + character.DEF * 0.4) / 50.0;
            winRate = Math.Clamp(winRate + (power - 1) * 0.15, 0.1, 0.95);

            bool victory = Data.Random.NextDouble() < winRate;

            // 模拟战斗过程文本
            WriteLine("战斗开始！");
            await Task.Delay(300);
            WriteLine($"敌方生命值：{Data.Random.Next(30, 80):0.##}");
            await Task.Delay(300);
            WriteLine($"你发起了攻击，造成了 {character.ATK + Data.Random.Next(-5, 10):0.##} 点伤害！");
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
        private void UpdateQuestProgress(string actionType)
        {
            foreach (Quest quest in Data.CurrentQuests.Where(q => q.Status == QuestState.InProgress))
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

        // 根据任务类型返回需要的房间类型（可根据实际Quest设计扩展）
        private InteractionPointType GetNeededRoomTypeForQuest(Quest quest)
        {
            if (quest.Name.Contains("战斗") || quest.NeedyExploreItemName?.Contains("战斗") == true)
                return InteractionPointType.General;

            if (quest.Name.Contains("精英") || quest.NeedyExploreItemName?.Contains("精英") == true)
                return InteractionPointType.Elite;

            if (quest.Name.Contains("宝箱") || quest.NeedyExploreItemName?.Contains("宝箱") == true)
                return InteractionPointType.Treasure;

            if (quest.Name.Contains("商店") || quest.NeedyExploreItemName?.Contains("商店") == true)
                return InteractionPointType.Store;

            // 默认返回普通战斗或事件
            return Data.Random.Next(2) == 0 ? InteractionPointType.General : InteractionPointType.Change;
        }

        // 按权重随机选择房间类型（可自行调整权重）
        private InteractionPointType ChooseRoomTypeByWeight(Dictionary<InteractionPointType, (int min, int max)> config, Dictionary<InteractionPointType, int> required)
        {
            // 示例权重：普通战最高，其次事件、宝箱等
            Dictionary<InteractionPointType, int> weights = new()
            {
                { InteractionPointType.General, 35 },
                { InteractionPointType.Change,  20 },
                { InteractionPointType.Treasure,15 },
                { InteractionPointType.Elite,   18 },
                { InteractionPointType.Rest,    7 },
                { InteractionPointType.Store,    5 }
            };

            int totalWeight = weights.Values.Sum();
            int roll = Data.Random.Next(totalWeight);

            int current = 0;
            foreach (var (type, weight) in weights)
            {
                current += weight;
                if (roll < current)
                    return type;
            }
            return InteractionPointType.General;
        }

        // 统计当前地图中某种房间的数量
        private static int CountRoomType(GameMap map, InteractionPointType type)
        {
            return map.Grids.Values.Count(g =>
                g.InteractionPoints.Any(ip => (InteractionPointType)ip.CustomValue == type));
        }

        // 添加交互点
        private void AddInteractionPoint(Grid grid, InteractionPointType type)
        {
            InteractionPoint ip = new()
            {
                Id = grid.Id,
                CustomValue = (int)type
            };
            grid.InteractionPoints.Add(ip);

            grid.CharacterEntered += (character) =>
            {
                HandleCharacterEnteredGrid(character, grid, ip);  // 注意：data 需要通过闭包或字段传递
            };
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

        private List<Quest> GetQuests(OshimaRegion region, int count)
        {
            List<Quest> quests = [];

            int immediateQuestCount = Data.Random.Next(count + 1);
            int progressiveQuestCount = count - immediateQuestCount;

            var list = region.ImmediateQuestList.OrderBy(kv => Data.Random.Next()).Take(immediateQuestCount);
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
            list = region.ProgressiveQuestList.OrderBy(kv => Data.Random.Next()).Take(progressiveQuestCount);
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
