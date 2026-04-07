using System.Text;
using Microsoft.Extensions.Logging;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface.Base;
using Milimoe.FunGame.Core.Library.Common.Addon;
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
            WriteLine("正在降落...");
            data.CurrentArea = data.CurrentRegion.Areas.OrderByDescending(o => data.Random.Next()).First();
            WriteLine($"在【{data.CurrentRegion.Name}】的【{data.CurrentArea}】区域完成降落！");
            List<Quest> quests = GetQuests(data, data.CurrentRegion, 2);
            data.CurrentQuests = quests;
            AddDialog("柔哥", $"我看到您安全着陆了，现在方舟给您下发任务指令：\r\n{string.Join("\r\n", quests.Select(q => q.ToString()))}");
            bool fin = false;
            while (!fin)
            {
                if (data.CurrentMap != null)
                {
                    DisplayMapInConsole(data.CurrentMap);
                }
                // TODO:开始探索区域，主要抉择
                fin = true;
            }
            newState = chapter switch
            {
                1 => RogueState.ArriveChapter1RallyPoint,
                2 => RogueState.ArriveChapter2RallyPoint,
                3 => RogueState.ArriveChapter3RallyPoint,
                _ => RogueState.Finish,
            };
            return newState;
        }

        private static List<Quest> GetQuests(RogueLikeGameData data, OshimaRegion region, int count)
        {
            List<Quest> quests = [];

            int immediateQuestCount = data.Random.Next(count + 1);
            int progressiveQuestCount = count - immediateQuestCount;

            var list = region.ImmediateQuestList.OrderBy(kv => data.Random.Next()).Take(count);
            foreach (var item in list)
            {
                string name = region.ImmediateQuestList.Keys.OrderBy(o => Random.Shared.Next()).First();
                QuestExploration exploration = region.ImmediateQuestList[name];
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
            list = region.ProgressiveQuestList.OrderBy(kv => data.Random.Next()).Take(count);
            foreach (var item in list)
            {
                string name = region.ProgressiveQuestList.Keys.OrderBy(o => Random.Shared.Next()).First();
                QuestExploration exploration = region.ProgressiveQuestList[name];
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
                sb.Append($" {x}  ");
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
                        InteractionPoint ip = grid.InteractionPoints.First();
                        string displayChar = ((InteractionPointType)ip.CustomValue).ToString();
                        displayChar = displayChar.Length > 0 ? displayChar[0].ToString().ToUpper() : "?";
                        sb.Append($"<{displayChar}> ");
                    }
                    else
                    {
                        sb.Append(" .  ");
                    }
                    if (x == map.Length - 1) sb.Append('\t');
                }
            }

            WriteLine(sb.ToString());
        }

        private static void SetupGrid(RogueLikeGameData data, GameMap map)
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
                switch ((InteractionPointType)ip.CustomValue)
                {
                    case InteractionPointType.General:
                        grid.CharacterEntered += (character) =>
                        {

                        };
                        break;
                    case InteractionPointType.Elite:
                        grid.CharacterEntered += (character) =>
                        {

                        };
                        break;
                    case InteractionPointType.Store:
                        grid.CharacterEntered += (character) =>
                        {

                        };
                        break;
                    case InteractionPointType.Treasure:
                        grid.CharacterEntered += (character) =>
                        {

                        };
                        break;
                    case InteractionPointType.Rest:
                        grid.CharacterEntered += (character) =>
                        {

                        };
                        break;
                    case InteractionPointType.Change:
                        grid.CharacterEntered += (character) =>
                        {

                        };
                        break;
                    default:
                        break;
                }
            }
            Grid? land = map.Grids.Values.OrderBy(g => random.Next()).FirstOrDefault(g => g.InteractionPoints.Count == 0);
            if (land != null)
            {
                map.SetCharacterCurrentGrid(data.Character, land);
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
