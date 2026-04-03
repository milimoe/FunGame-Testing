using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Milimoe.FunGame.Testing.Tests;
using Oshima.FunGame.OshimaModules.Characters;
using Oshima.FunGame.OshimaModules.Models;
using Oshima.FunGame.OshimaModules.Regions;

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

        public void ReceiveDataRequest(Guid guid, DataRequestArgs args)
        {
            Inquiries.Add((guid, args));
        }

        public void WriteLine(string message = "") => Dispatcher.WriteLine(message);

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
                        string username = "";
                        if (args.Data.TryGetValue("username", out object? value) && value is string s)
                        {
                            username = s;
                        }
                        User user = Factory.GetUser(1, username);
                        Character character = new CustomCharacter(user.Id, username)
                        {
                            Level = 1
                        };
                        character.Recovery();
                        user.Inventory.Characters.Add(character);
                        user.Inventory.MainCharacter = character;
                        Users[username] = user;
                        response.Data["user"] = user;
                        break;
                    }
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
                        OshimaRegion? region = await ChooseRegion(1);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅰ：启动 --");
                            data.CurrentRegion = region;
                            data.Chapter1Region = region;
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
                        newState = RogueState.ExploringChapter1Area1;
                    }
                    break;
                case RogueState.Chapter2InArk:
                    {
                        WriteLine("回到方舟后，我们得到了新的任务。");
                        // TODO:提供一个菜单，玩家可以选择第二章的地区（随机从3-4★的地区里抽取3个来选一个）
                        OshimaRegion? region = await ChooseRegion(2);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅱ：启动 --");
                            data.CurrentRegion = region;
                            data.Chapter2Region = region;
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                            WriteLine("结算到目前为止的奖励。");
                            break;
                        }
                        await RestInArk();
                        newState = RogueState.ExploringChapter2Area1;
                    }
                    break;
                case RogueState.Chapter3InArk:
                    {
                        OshimaRegion? region = await ChooseRegion(3);
                        if (region != null)
                        {
                            WriteLine("-- 【永恒方舟计划】进程·Ⅲ：启动 --");
                            data.CurrentRegion = region;
                            data.Chapter3Region = region;
                        }
                        else
                        {
                            WriteLine("未选择地区，永恒方舟计划终止。");
                            newState = RogueState.Finish;
                            WriteLine("结算到目前为止的奖励。");
                            break;
                        }
                        await RestInArk();
                        newState = RogueState.ExploringChapter3Area1;
                    }
                    break;
                case RogueState.FinalInArk:
                    WriteLine("-- 【永恒方舟计划】紧急事件·夺还：启动 --");
                    newState = RogueState.ExploringArk;
                    break;
                case RogueState.ExploringChapter1Area1:
                    newState = await ExploreRegion(data, 1, 1);
                    break;
                case RogueState.ExploringChapter1Area2:
                    newState = await ExploreRegion(data, 1, 2);
                    break;
                case RogueState.ExploringChapter2Area1:
                    newState = await ExploreRegion(data, 2, 1);
                    break;
                case RogueState.ExploringChapter2Area2:
                    newState = await ExploreRegion(data, 2, 2);
                    break;
                case RogueState.ExploringChapter3Area1:
                    newState = await ExploreRegion(data, 3, 1);
                    break;
                case RogueState.ExploringChapter3Area2:
                    newState = await ExploreRegion(data, 3, 2);
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

        private async Task<OshimaRegion?> ChooseRegion(int chapter)
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
                Choices = FunGameConstant.Regions.Where(predicate).ToDictionary(r => r.Name, r => r.ToString())
            };
            InquiryResponse response = await Dispatcher.GetInGameResponse(options);
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

        private async Task<RogueState> ExploreRegion(RogueLikeGameData data, int chapter, int areaSeq)
        {
            RogueState newState = RogueState.Finish;
            if (data.CurrentRegion is null)
            {
                WriteLine("突发！小队和方舟失联了！永恒方舟计划终止。");
                return newState;
            }
            if (areaSeq == 1)
            {
                WriteLine("正在降落...");
                data.CurrentArea = data.CurrentRegion.Areas.OrderByDescending(o => Random.Shared.Next()).First();
                WriteLine($"在【{data.CurrentRegion.Name}】的【{data.CurrentArea}】区域完成降落！");
            }
            else if (areaSeq == 2)
            {
                WriteLine("该地区的第一个探索区域任务已完成！正在前往第二个区域……");
                data.CurrentArea = data.CurrentRegion.Areas.OrderByDescending(o => Random.Shared.Next()).First(a => a != data.CurrentArea);
                WriteLine($"在【{data.CurrentRegion.Name}】的【{data.CurrentArea}】区域完成降落！");
            }
            else
            {
                WriteLine("你误入了神秘地带，与方舟失联，游戏结束。");
                return newState;
            }
            bool fin = false;
            while (!fin)
            {
                // TODO:开始探索区域，主要抉择
                fin = true;
            }
            if (areaSeq == 1)
            {
                newState = chapter switch
                {
                    1 => RogueState.ExploringChapter1Area2,
                    2 => RogueState.ExploringChapter2Area2,
                    3 => RogueState.ExploringChapter3Area2,
                    _ => RogueState.Finish,
                };
            }
            else if (areaSeq == 2)
            {
                WriteLine("BOSS房间出现了！做好准备再继续出发吧。");
                fin = false;
                while (!fin)
                {
                    // TODO:BOSS房前的准备，提供菜单
                    fin = true;
                }
                newState = chapter switch
                {
                    1 => RogueState.Chapter1BossBattle,
                    2 => RogueState.Chapter2BossBattle,
                    3 => RogueState.Chapter3BossBattle,
                    _ => RogueState.Finish,
                };
            }
            else
            {
                WriteLine("你误入了神秘地带，与方舟失联，游戏结束。");
            }
            return newState;
        }

        private async Task RestInArk()
        {
            bool fin = false;
            while (!fin)
            {
                // TODO:战后整备，提供菜单回复和提升等
                fin = true;
            }
            WriteLine("出发！");
        }

        private async Task ExploreArk()
        {
            bool fin = false;
            while (!fin)
            {
                // TODO:方舟事变，需进行方舟探索和收复功能房间并找到最终BOSS房间
                fin = true;
            }
            WriteLine("出发！");
        }
    }

    public enum RogueState
    {
        Init,
        InArk,
        Chapter1InArk,
        Chapter2InArk,
        Chapter3InArk,
        FinalInArk,
        ExploringChapter1Area1,
        ExploringChapter1Area2,
        ExploringChapter2Area1,
        ExploringChapter2Area2,
        ExploringChapter3Area1,
        ExploringChapter3Area2,
        ExploringArk,
        Chapter1BossBattle,
        Chapter2BossBattle,
        Chapter3BossBattle,
        FinalBossBattle,
        Finish
    }
}
