using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    public class FunGameBO5
    {
        public static async Task StartBO5()
        {
            List<User> userlist = [];
            while (userlist.Count != 10)
            {
                User user = Factory.GetUser();
                user.Username = FunGameService.GenerateRandomChineseUserName();
                userlist.Add(user);
            }

            HashSet<User> u1 = [];
            HashSet<User> u2 = [];
            Dictionary<HashSet<User>, string> userTeams = new()
            {
                { u1, "1" },
                { u2, "2" }
            };

            int i = 0;
            while (true)
            {
                // 入队
                User? x = userlist.FirstOrDefault(u => !u1.Contains(u) && !u2.Contains(u));
                if (x is null) continue;
                if (i % 2 == 0 && u1.Count < 5)
                {
                    u1.Add(x);
                    if (u1.Count == 1)
                    {
                        userTeams[u1] = FunGameService.GenerateRandomChineseUserName();
                    }
                    Console.WriteLine($"{x} 加入了 {userTeams[u1]}");
                }
                if (i % 2 != 0 && u2.Count < 5)
                {
                    u2.Add(x);
                    if (u2.Count == 1)
                    {
                        userTeams[u2] = FunGameService.GenerateRandomChineseUserName();
                    }
                    Console.WriteLine($"{x} 加入了 {userTeams[u2]}");
                }
                i++;
                if (u1.Count == 5 && u2.Count == 5)
                {
                    break;
                }
            }

            Console.WriteLine("\r\n\r\nBO5系列赛，赛制：团队死斗模式，目标30人头。\r\n\r\n");

            Dictionary<HashSet<User>, int> teamScore = new()
            {
                { u1, 0 },
                { u2, 0 }
            };
            Dictionary<User, CharacterStatistics> stats = [];

            int round = 1;
            while (true)
            {
                Console.WriteLine($"【第 {round++} 局】Live!Live!Live! B/P 阶段开始！当前大比分：{userTeams[u1]}【{teamScore[u1]} - {teamScore[u2]}】{userTeams[u2]}");

                Dictionary<User, Character> t1 = [];
                Dictionary<User, Character> t2 = [];
                HashSet<Character> ban = [];

                Team team1 = new(userTeams[u1], []);
                Team team2 = new(userTeams[u2], []);

                // 打乱顺序
                IEnumerable<Character> xx = FunGameConstant.Characters.OrderBy(o => Random.Shared.Next());
                i = 0;
                while (true)
                {
                    // 禁用
                    if (ban.Count < 2)
                    {
                        Character? x = xx.FirstOrDefault(c => !ban.Contains(c));
                        if (x is null) continue;
                        if (i % 2 == 0)
                        {
                            ban.Add(x);
                            Console.WriteLine($"{userTeams[u1]} 禁用了 {x}");
                        }
                        else if (i % 2 != 0)
                        {
                            ban.Add(x);
                            Console.WriteLine($"{userTeams[u2]} 禁用了 {x}");
                        }
                    }
                    else
                    {
                        // 选秀
                        Character? x = xx.FirstOrDefault(c => !t1.ContainsValue(c) && !t2.ContainsValue(c) && !ban.Contains(c));
                        if (x is null) continue;
                        Character c = x.Copy();
                        c.Level = 60;
                        c.NormalAttack.Level = 8;
                        FunGameService.AddCharacterSkills(c, 1, 6, 6);
                        if (i % 2 == 0 && t1.Count < 5)
                        {
                            User? uu = u1.FirstOrDefault(u => !t1.ContainsKey(u));
                            if (uu is null) continue;
                            stats.TryAdd(uu, new());
                            t1[uu] = x;
                            team1.Members.Add(c);
                            Console.WriteLine($"{userTeams[u1]}.{uu} 选择了 {c}");
                            c.User = uu;
                        }
                        if (i % 2 != 0 && t2.Count < 5)
                        {
                            User? uu = u2.FirstOrDefault(u => !t2.ContainsKey(u));
                            if (uu is null) continue;
                            stats.TryAdd(uu, new());
                            t2[uu] = x;
                            team2.Members.Add(c);
                            Console.WriteLine($"{userTeams[u2]}.{uu} 选择了 {c}");
                            c.User = uu;
                        }
                    }
                    i++;
                    if (t1.Count == 5 && t2.Count == 5)
                    {
                        break;
                    }
                }

                List<Team> teams = [team1, team2];
                Console.WriteLine("\r\n\r\n");
                foreach (Team team in teams)
                {
                    Console.WriteLine($"战队【{team}】的队员：\r\n{string.Join("\r\n", team.Members.Select(c => c.ToStringWithUser()))}\r\n");
                }

                DropItems(team1.Members);
                DropItems(team2.Members);
                team1.Members.ForEach(c => c.Recovery());
                team2.Members.ForEach(c => c.Recovery());
                FunGameActionQueue queue = new();
                List<string> msgs = await queue.StartTeamGame(teams, -1, 30);
                foreach (Character character in queue.ActionQueue.CharacterStatistics.Keys)
                {
                    Milimoe.FunGame.Testing.Tests.FunGameSimulation.UpdateStatistics(stats[character.User], queue.ActionQueue.CharacterStatistics[character]);
                }
                Console.WriteLine(string.Join("\r\n\r\n", msgs[^2..]));
                foreach (Team team in queue.ActionQueue.EliminatedTeams)
                {
                    if (team.IsWinner && team.Name == userTeams[u1]) teamScore[u1]++;
                    if (team.IsWinner && team.Name == userTeams[u2]) teamScore[u2]++;
                }
                if (teamScore[u1] == 3)
                {
                    Console.WriteLine($"恭喜【{userTeams[u1]}】以【{teamScore[u1]} - {teamScore[u2]}】的战绩获得了比赛的胜利！");
                    break;
                }
                if (teamScore[u2] == 3)
                {
                    Console.WriteLine($"恭喜【{userTeams[u2]}】以【{teamScore[u2]} - {teamScore[u1]}】的战绩获得了比赛的胜利！");
                    break;
                }
            }

            Console.WriteLine($"评选最佳选手中……表格格式：[Rating] Team.Name K/A/D\r\n-------------------------------------\r\n\r\n");
            bool isFirst = true;
            User? mvp = null;
            string mTeam = "";
            foreach (User u in stats.OrderByDescending(kv => kv.Value.Rating).Select(kv => kv.Key))
            {
                CharacterStatistics stat = stats[u];
                string team = userTeams[userTeams.Keys.First(h => h.Contains(u))];
                Console.WriteLine($"[{stat.Rating:0.0#}] \t{team}.{u} \t{stat.Kills} \t/ \t{stat.Assists} \t/ \t{stat.Deaths}\r\n");
                if (isFirst)
                {
                    isFirst = false;
                    mvp = u;
                    mTeam = team;
                }
            }
            if (mvp != null) Console.WriteLine($"恭喜【{mTeam}】战队的选手【{mvp}】获得了本次比赛的最佳选手称号！");
        }

        public static void DropItems(IEnumerable<Character> characters)
        {
            foreach (Character character in characters)
            {
                Item[] 武器 = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("11") && (int)i.QualityType == 4)];
                Item[] 防具 = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("12") && (int)i.QualityType == 1)];
                Item[] 鞋子 = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("13") && (int)i.QualityType == 1)];
                Item[] 饰品1 = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("14") && (int)i.QualityType == 3)];
                Item[] 饰品2 = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("14") && (int)i.QualityType == 3)];
                Item? a = null, b = null, c = null, d = null, e = null;
                if (武器.Length > 0)
                {
                    a = 武器[Random.Shared.Next(武器.Length)];
                }
                if (防具.Length > 0)
                {
                    b = 防具[Random.Shared.Next(防具.Length)];
                }
                if (鞋子.Length > 0)
                {
                    c = 鞋子[Random.Shared.Next(鞋子.Length)];
                }
                if (饰品1.Length > 0)
                {
                    d = 饰品1[Random.Shared.Next(饰品1.Length)];
                }
                if (饰品2.Length > 0)
                {
                    e = 饰品2[Random.Shared.Next(饰品2.Length)];
                }
                List<Item> 这次发放的空投 = [];
                if (a != null) 这次发放的空投.Add(a);
                if (b != null) 这次发放的空投.Add(b);
                if (c != null) 这次发放的空投.Add(c);
                if (d != null) 这次发放的空投.Add(d);
                if (e != null) 这次发放的空投.Add(e);
                Item? 魔法卡包 = FunGameService.GenerateMagicCardPack(4, QualityType.Orange);
                if (魔法卡包 != null)
                {
                    foreach (Skill magic in 魔法卡包.Skills.Magics)
                    {
                        magic.Level = 8;
                    }
                    character.Equip(魔法卡包);
                }
                foreach (Item item in 这次发放的空投)
                {
                    Item realItem = item.Copy();
                    character.Equip(realItem);
                }
            }
        }
    }
}
