using System.Text;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Oshima.Core.Constant;
using Oshima.FunGame.OshimaModules.Effects.OpenEffects;
using Oshima.FunGame.OshimaModules.Skills;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    public class FunGameTesting
    {
        public FunGameTesting()
        {
            InitCharacter();
            Task.Run(async () =>
            {
                bool printout = true;
                List<string> strs = await StartGame(printout);
                if (printout == false)
                {
                    foreach (string str in strs)
                    {
                        Console.WriteLine(str);
                    }
                }

                Console.ReadKey();
            });
        }

        public static Dictionary<Character, CharacterStatistics> CharacterStatistics { get; } = [];
        public static PluginConfig StatsConfig { get; } = new(nameof(FunGameSimulation), nameof(CharacterStatistics));
        public static bool IsRuning { get; set; } = false;
        public static bool IsWeb { get; set; } = false;
        public static bool PrintOut { get; set; } = false;
        public static string Msg { get; set; } = "";

        public static async Task<List<string>> StartGame(bool printout, bool isWeb = false)
        {
            PrintOut = printout;
            IsWeb = isWeb;
            try
            {
                if (IsRuning) return ["游戏正在模拟中，请勿重复请求！"];

                List<string> result = [];
                Msg = "";

                IsRuning = true;

                // M = 0, W = 7, P1 = 1, P3 = 1
                // M = 1, W = 6, P1 = 2, P3 = 0
                // M = 2, W = 4, P1 = 0, P3 = 2
                // M = 2, W = 5, P1 = 0, P3 = 0
                // M = 3, W = 3, P1 = 1, P3 = 1
                // M = 4, W = 2, P1 = 2, P3 = 0
                // M = 5, W = 0, P1 = 0, P3 = 2
                // M = 5, W = 1, P1 = 0, P3 = 0

                List<Character> list = [.. FunGameConstant.Characters];

                if (PrintOut) Console.WriteLine();
                if (PrintOut) Console.WriteLine("Start!!!");
                if (PrintOut) Console.WriteLine();

                int clevel = 60;
                int slevel = 6;
                int mlevel = 8;

                // 升级和赋能
                List<Character> characters = [];
                for (int index = 0; index < FunGameConstant.Characters.Count; index++)
                {
                    Character c = FunGameConstant.Characters[index];
                    c.Level = clevel;
                    c.NormalAttack.Level = mlevel;
                    FunGameService.AddCharacterSkills(c, 1, slevel, slevel);
                    Skill 疾风步 = new 疾风步(c)
                    {
                        Level = slevel
                    };
                    c.Skills.Add(疾风步);
                    characters.Add(c);
                }

                // 显示角色信息
                if (PrintOut) characters.ForEach(c => Console.WriteLine($"角色编号：{c.Id}\r\n{c.GetInfo()}"));

                // 询问玩家需要选择哪个角色
                Character? player = null;
                await Task.Run(() =>
                {
                    while (player is null)
                    {
                        Console.Write("请选择你想玩的角色（输入角色编号）：");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int id) && characters.FirstOrDefault(c => c.Id == id) is Character c)
                        {
                            player = c;
                            Console.WriteLine($"选择了 [ {player} ]！");
                            Console.WriteLine($"按任意键继续. . .");
                            Console.ReadKey();
                            break;
                        }
                    }
                });

                if (player is null)
                {
                    throw new Exception("没有选择角色，游戏结束。");
                }

                // 创建顺序表并排序
                MixGamingQueue gamingQueue = new(characters, WriteLine)
                {
                    GameplayEquilibriumConstant = OshimaGameModuleConstant.GameplayEquilibriumConstant
                };
                if (PrintOut) Console.WriteLine();

                // 绑定事件
                gamingQueue.TurnStart += GamingQueue_TurnStart;
                gamingQueue.DecideAction += GamingQueue_DecideAction;
                gamingQueue.SelectNormalAttackTargets += GamingQueue_SelectNormalAttackTargets;
                gamingQueue.SelectSkill += GamingQueue_SelectSkill;
                gamingQueue.SelectSkillTargets += GamingQueue_SelectSkillTargets;
                gamingQueue.SelectItem += GamingQueue_SelectItem;
                gamingQueue.QueueUpdated += GamingQueue_QueueUpdated;
                gamingQueue.TurnEnd += GamingQueue_TurnEnd;

                // 总游戏时长
                double totalTime = 0;

                // 开始空投
                Msg = "";
                int qMagicCardPack = 0;
                int qWeapon = 0;
                int qArmor = 0;
                int qShoes = 0;
                int qAccessory = 0;
                WriteLine($"社区送温暖了，现在随机发放空投！！");
                DropItems(gamingQueue, qMagicCardPack, qWeapon, qArmor, qShoes, qAccessory);
                WriteLine("");
                if (isWeb) result.Add("=== 空投 ===\r\n" + Msg);
                double nextDropItemTime = 40;
                if (qMagicCardPack < 4)
                {
                    qMagicCardPack++;
                }
                if (qWeapon < 4)
                {
                    qWeapon++;
                }
                if (qArmor < 1)
                {
                    qArmor++;
                }
                if (qShoes < 1)
                {
                    qShoes++;
                }
                if (qAccessory < 3)
                {
                    qAccessory++;
                }

                // 显示角色信息
                if (PrintOut) characters.ForEach(c => Console.WriteLine(c.GetInfo()));

                // 因赋予了装备，所以清除排序重新排
                gamingQueue.ClearQueue();
                gamingQueue.InitCharacterQueue(characters);
                gamingQueue.SetCharactersToAIControl(false, characters);
                gamingQueue.SetCharactersToAIControl(true, player);
                gamingQueue.CustomData.Add("player", player);
                if (PrintOut) Console.WriteLine();

                // 显示初始顺序表
                gamingQueue.DisplayQueue();
                if (PrintOut) Console.WriteLine();

                Console.WriteLine($"你的角色是 [ {player} ]，详细信息：{player.GetInfo()}");
                Console.WriteLine($"按任意键继续. . .");
                Console.ReadKey();

                // 总回合数
                int maxRound = 999;

                // 随机回合奖励
                Dictionary<long, bool> effects = [];
                foreach (EffectID id in FunGameConstant.RoundRewards.Keys)
                {
                    long effectID = (long)id;
                    bool isActive = false;
                    if (effectID > (long)EffectID.Active_Start)
                    {
                        isActive = true;
                    }
                    effects.Add(effectID, isActive);
                }
                gamingQueue.InitRoundRewards(maxRound, 1, effects, id => FunGameConstant.RoundRewards[(EffectID)id]);

                int i = 1;
                while (i < maxRound)
                {
                    Msg = "";
                    if (i == maxRound - 1)
                    {
                        WriteLine($"=== 终局审判 ===");
                        Dictionary<Character, double> hpPercentage = [];
                        foreach (Character c in characters)
                        {
                            hpPercentage.TryAdd(c, c.HP / c.MaxHP);
                        }
                        double max = hpPercentage.Values.Max();
                        Character winner = hpPercentage.Keys.Where(c => hpPercentage[c] == max).First();
                        WriteLine("[ " + winner + " ] 成为了天选之人！！");
                        foreach (Character c in characters.Where(c => c != winner && c.HP > 0))
                        {
                            WriteLine("[ " + winner + " ] 对 [ " + c + " ] 造成了 99999999999 点真实伤害。");
                            await gamingQueue.DeathCalculationAsync(winner, c);
                        }
                        await gamingQueue.EndGameInfo(winner);
                        result.Add(Msg);
                        break;
                    }

                    // 检查是否有角色可以行动
                    Character? characterToAct = await gamingQueue.NextCharacterAsync();

                    // 处理回合
                    if (characterToAct != null)
                    {
                        WriteLine($"=== Round {i++} ===");
                        WriteLine("现在是 [ " + characterToAct + " ] 的回合！");

                        bool isGameEnd = await gamingQueue.ProcessTurnAsync(characterToAct);

                        if (isGameEnd)
                        {
                            result.Add(Msg);
                            break;
                        }

                        if (isWeb) gamingQueue.DisplayQueue();
                        WriteLine("");
                    }

                    string roundMsg = "";
                    if (gamingQueue.LastRound.HasKill)
                    {
                        roundMsg = Msg;
                        Msg = "";
                    }

                    // 模拟时间流逝
                    double timeLapse = await gamingQueue.TimeLapse();
                    totalTime += timeLapse;
                    nextDropItemTime -= timeLapse;

                    if (roundMsg != "")
                    {
                        if (isWeb)
                        {
                            roundMsg += "\r\n" + Msg;
                        }
                        result.Add(roundMsg);
                    }

                    if (nextDropItemTime <= 0)
                    {
                        // 空投
                        Msg = "";
                        WriteLine($"社区送温暖了，现在随机发放空投！！");
                        DropItems(gamingQueue, qMagicCardPack, qWeapon, qArmor, qShoes, qAccessory);
                        WriteLine("");
                        if (isWeb) result.Add("=== 空投 ===\r\n" + Msg);
                        nextDropItemTime = 40;
                        if (qMagicCardPack < 4)
                        {
                            qMagicCardPack++;
                        }
                        if (qWeapon < 4)
                        {
                            qWeapon++;
                        }
                        if (qArmor < 1)
                        {
                            qArmor++;
                        }
                        if (qShoes < 1)
                        {
                            qShoes++;
                        }
                        if (qAccessory < 3)
                        {
                            qAccessory++;
                        }
                    }
                }

                if (PrintOut)
                {
                    Console.WriteLine("--- End ---");
                    Console.WriteLine($"总游戏时长：{totalTime:0.##} {gamingQueue.GameplayEquilibriumConstant.InGameTime}");
                    Console.WriteLine("");
                }

                // 赛后统计
                FunGameService.GetCharacterRating(gamingQueue.CharacterStatistics, false, []);

                // 统计技术得分，评选 MVP
                Character? mvp = gamingQueue.CharacterStatistics.OrderByDescending(d => d.Value.Rating).Select(d => d.Key).FirstOrDefault();
                StringBuilder mvpBuilder = new();
                if (mvp != null)
                {
                    CharacterStatistics stats = gamingQueue.CharacterStatistics[mvp];
                    stats.MVPs++;
                    mvpBuilder.AppendLine($"[ {mvp.ToStringWithLevel()} ]");
                    mvpBuilder.AppendLine($"技术得分：{stats.Rating:0.0#} / 击杀数：{stats.Kills} / 助攻数：{stats.Assists}{(gamingQueue.MaxRespawnTimes != 0 ? " / 死亡数：" + stats.Deaths : "")}");
                    mvpBuilder.AppendLine($"存活时长：{stats.LiveTime:0.##} / 存活回合数：{stats.LiveRound} / 行动回合数：{stats.ActionTurn}");
                    mvpBuilder.AppendLine($"控制时长：{stats.ControlTime:0.##} / 总计治疗：{stats.TotalHeal:0.##} / 护盾抵消：{stats.TotalShield:0.##}");
                    mvpBuilder.AppendLine($"总计伤害：{stats.TotalDamage:0.##} / 总计物理伤害：{stats.TotalPhysicalDamage:0.##} / 总计魔法伤害：{stats.TotalMagicDamage:0.##}");
                    mvpBuilder.AppendLine($"总承受伤害：{stats.TotalTakenDamage:0.##} / 总承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 总承受魔法伤害：{stats.TotalTakenMagicDamage:0.##}");
                    mvpBuilder.Append($"每秒伤害：{stats.DamagePerSecond:0.##} / 每回合伤害：{stats.DamagePerTurn:0.##}");
                }

                int top = isWeb ? gamingQueue.CharacterStatistics.Count : 0; // 回执多少个角色的统计信息
                int count = 1;
                if (isWeb)
                {
                    WriteLine("=== 技术得分排行榜 ==="); // 这是输出在界面上的
                    Msg = $"=== 技术得分排行榜 TOP{top} ===\r\n"; // 这个是下一条给Result回执的标题，覆盖掉上面方法里的赋值了
                }
                else
                {
                    StringBuilder ratingBuilder = new();
                    WriteLine("=== 本场比赛最佳角色 ===");
                    Msg = $"=== 本场比赛最佳角色 ===\r\n";
                    WriteLine(mvpBuilder.ToString() + "\r\n\r\n" + ratingBuilder.ToString());

                    if (PrintOut)
                    {
                        Console.WriteLine();
                        Console.WriteLine("=== 技术得分排行榜 ===");
                    }
                }

                foreach (Character character in gamingQueue.CharacterStatistics.OrderByDescending(d => d.Value.Rating).Select(d => d.Key))
                {
                    StringBuilder builder = new();
                    CharacterStatistics stats = gamingQueue.CharacterStatistics[character];
                    builder.AppendLine($"{(isWeb ? count + ". " : "")}[ {character.ToStringWithLevel()} ]");
                    builder.AppendLine($"技术得分：{stats.Rating:0.0#} / 击杀数：{stats.Kills} / 助攻数：{stats.Assists}{(gamingQueue.MaxRespawnTimes != 0 ? " / 死亡数：" + stats.Deaths : "")}");
                    builder.AppendLine($"存活时长：{stats.LiveTime:0.##} / 存活回合数：{stats.LiveRound} / 行动回合数：{stats.ActionTurn}");
                    builder.AppendLine($"控制时长：{stats.ControlTime:0.##} / 总计治疗：{stats.TotalHeal:0.##} / 护盾抵消：{stats.TotalShield:0.##}");
                    builder.AppendLine($"总计伤害：{stats.TotalDamage:0.##} / 总计物理伤害：{stats.TotalPhysicalDamage:0.##} / 总计魔法伤害：{stats.TotalMagicDamage:0.##}");
                    builder.AppendLine($"总承受伤害：{stats.TotalTakenDamage:0.##} / 总承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 总承受魔法伤害：{stats.TotalTakenMagicDamage:0.##}");
                    builder.Append($"每秒伤害：{stats.DamagePerSecond:0.##} / 每回合伤害：{stats.DamagePerTurn:0.##}");
                    if (count++ <= top)
                    {
                        WriteLine(builder.ToString());
                    }
                    else
                    {
                        if (PrintOut) Console.WriteLine(builder.ToString());
                    }

                    CharacterStatistics? totalStats = CharacterStatistics.Where(kv => kv.Key.GetName() == character.GetName()).Select(kv => kv.Value).FirstOrDefault();
                    if (totalStats != null)
                    {
                        UpdateStatistics(totalStats, stats);
                    }
                }
                result.Add(Msg);

                // 显示每个角色的信息
                if (isWeb)
                {
                    for (i = gamingQueue.Eliminated.Count - 1; i >= 0; i--)
                    {
                        Character character = gamingQueue.Eliminated[i];
                        result.Add($"=== 角色 [ {character} ] ===\r\n{character.GetInfo()}");
                    }
                }

                lock (StatsConfig)
                {
                    foreach (Character c in CharacterStatistics.Keys)
                    {
                        StatsConfig.Add(c.ToStringWithOutUser(), CharacterStatistics[c]);
                    }
                    StatsConfig.SaveConfig();
                }

                IsRuning = false;

                return result;
            }
            catch (Exception ex)
            {
                IsRuning = false;
                Console.WriteLine(ex);
                return [ex.ToString()];
            }
        }

        private static async Task GamingQueue_QueueUpdated(GamingQueue queue, List<Character> characters, Character character, double hardnessTime, QueueUpdatedReason reason, string msg)
        {
            if (IsPlayer_OnlyTest(queue, character))
            {
                if (reason == QueueUpdatedReason.Action)
                {
                    // QueueUpdatedReason.Action 是指角色行动后改变了顺序表
                    // 调用 AI 托管，这里目的是，因为没有GUI，为了让角色能在其他角色的行动回合可以使用爆发技插队，这里需要让AI去释放。
                    queue.SetCharactersToAIControl(false, character);
                }
                if (reason == QueueUpdatedReason.PreCastSuperSkill)
                {
                    // QueueUpdatedReason.PreCastSuperSkill 是指角色释放了爆发技
                    // 通知玩家，让玩家知道下一次行动需要选择目标
                    Console.WriteLine($"你的下一回合需要选择爆发技目标，知晓请按任意键继续. . .");
                    await Console.In.ReadLineAsync();
                }
            }
            await Task.CompletedTask;
        }

        private static async Task<bool> GamingQueue_TurnStart(GamingQueue queue, Character character, List<Character> enemys, List<Character> teammates, List<Skill> skills, List<Item> items)
        {
            if (IsPlayer_OnlyTest(queue, character))
            {
                // 结束 AI 托管
                queue.SetCharactersToAIControl(cancel: true, character);
                await Task.CompletedTask;
            }
            // 注意，此事件返回 false 将全程接管此回合。
            return true;
        }

        private static async Task<List<Character>> GamingQueue_SelectNormalAttackTargets(GamingQueue queue, Character character, NormalAttack attack, List<Character> enemys, List<Character> teammates)
        {
            List<Character> characters = [];
            if (IsPlayer_OnlyTest(queue, character))
            {
                await Task.Run(() =>
                {
                    Console.WriteLine($"你的角色编号：{character.GetIdName()}");
                    Console.WriteLine("【敌对角色列表】" + "\r\n" + string.Join("\r\n", enemys.Select(c => $"{c.GetIdName()}：{c.GetSimpleInBattleInfo(queue.HardnessTime[c])}")));
                    Console.WriteLine("【友方角色列表】" + "\r\n" + string.Join("\r\n", teammates.Select(c => $"{c.GetIdName()}：{c.GetSimpleInBattleInfo(queue.HardnessTime[c])}")));
                    while (true)
                    {
                        if (characters.Count > attack.CanSelectTargetCount)
                        {
                            Console.WriteLine("选择角色过多，请重新输入。");
                            characters.Clear();
                        }
                        Console.Write($"请选择你想攻击的目标（输入角色编号，输入OK为确定）。\r\n可选择最多{attack.CanSelectTargetCount}个目标，已选{characters.Count}：");
                        string input = Console.ReadLine() ?? "";
                        if (input.Equals("OK", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("取消选择");
                            break;
                        }
                        if (int.TryParse(input, out int id))
                        {
                            if (id == 0)
                            {
                                break;
                            }
                            if (enemys.FirstOrDefault(c => c.Id == id) is Character enemy)
                            {
                                if (attack.CanSelectEnemy)
                                {
                                    characters.Add(enemy);
                                }
                                else
                                {
                                    Console.WriteLine("选择此角色失败：无法选择敌对角色。");
                                }
                            }
                            else if (teammates.FirstOrDefault(c => c.Id == id) is Character teammate)
                            {
                                if (attack.CanSelectTeammate)
                                {
                                    characters.Add(teammate);
                                }
                                else
                                {
                                    Console.WriteLine("选择此角色失败：无法选择友方角色。");
                                }
                            }
                            else if (id == character.Id)
                            {
                                if (attack.CanSelectSelf)
                                {
                                    characters.Add(character);
                                }
                                else
                                {
                                    Console.WriteLine("选择此角色失败：无法选择自己。");
                                }
                            }
                        }
                    }
                });
            }
            return characters;
        }

        private static async Task<Item?> GamingQueue_SelectItem(GamingQueue queue, Character character, List<Item> items)
        {
            Item? item = null;
            if (IsPlayer_OnlyTest(queue, character))
            {
                // 询问玩家需要选择哪个物品
                await Task.Run(() =>
                {
                    Console.WriteLine(string.Join("\r\n", items.Select(i => $"{i.GetIdName()}：{i}")));
                    while (item is null)
                    {
                        Console.Write("请选择你想使用的物品（输入物品编号，0为取消）：");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int id))
                        {
                            if (id == 0)
                            {
                                Console.WriteLine("取消选择");
                                break;
                            }
                            if (character.Items.FirstOrDefault(i => i.Id == id) is Item i)
                            {
                                item = i;
                                break;
                            }
                        }
                    }
                });
            }
            return item;
        }

        private static async Task<List<Character>> GamingQueue_SelectSkillTargets(GamingQueue queue, Character caster, Skill skill, List<Character> enemys, List<Character> teammates)
        {
            List<Character> characters = [];
            if (IsPlayer_OnlyTest(queue, caster))
            {
                await Task.Run(() =>
                {
                    Console.WriteLine($"你的角色编号：{caster.GetIdName()}");
                    Console.WriteLine("【敌对角色列表】" + "\r\n" + string.Join("\r\n", enemys.Select(c => $"{c.GetIdName()}：{c.GetSimpleInBattleInfo(queue.HardnessTime[c])}")));
                    Console.WriteLine("【友方角色列表】" + "\r\n" + string.Join("\r\n", teammates.Select(c => $"{c.GetIdName()}：{c.GetSimpleInBattleInfo(queue.HardnessTime[c])}")));
                    while (true)
                    {
                        if (characters.Count > skill.CanSelectTargetCount)
                        {
                            Console.WriteLine("选择角色过多，请重新输入。");
                            characters.Clear();
                        }
                        Console.Write($"请选择你想使用的技能目标（输入角色编号，输入OK为确定）。\r\n可选择最多{skill.CanSelectTargetCount}个目标，已选{characters.Count}：");
                        string input = Console.ReadLine() ?? "";
                        if (input.Equals("OK", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("结束选择");
                            break;
                        }
                        if (int.TryParse(input, out int id))
                        {
                            if (id == 0)
                            {
                                break;
                            }
                            if (enemys.FirstOrDefault(c => c.Id == id) is Character enemy)
                            {
                                if (skill.CanSelectEnemy)
                                {
                                    characters.Add(enemy);
                                }
                                else
                                {
                                    Console.WriteLine("选择此角色失败：该技能无法选择敌对角色。");
                                }
                            }
                            else if (teammates.FirstOrDefault(c => c.Id == id) is Character teammate)
                            {
                                if (skill.CanSelectTeammate)
                                {
                                    characters.Add(teammate);
                                }
                                else
                                {
                                    Console.WriteLine("选择此角色失败：该技能无法选择友方角色。");
                                }
                            }
                            else if (id == caster.Id)
                            {
                                if (skill.CanSelectSelf)
                                {
                                    characters.Add(caster);
                                }
                                else
                                {
                                    Console.WriteLine("选择此角色失败：该技能无法选择自己。");
                                }
                            }
                        }
                    }
                });
            }
            return characters;
        }

        private static async Task<Skill?> GamingQueue_SelectSkill(GamingQueue queue, Character character, List<Skill> skills)
        {
            Skill? skill = null;
            if (IsPlayer_OnlyTest(queue, character))
            {
                // 询问玩家需要选择哪个技能
                await Task.Run(() =>
                {
                    Console.WriteLine(string.Join("\r\n", skills.Select(s => $"{s.GetIdName()}：{s}")));
                    while (skill is null)
                    {
                        Console.Write("请选择你想使用的技能（输入技能编号，0为取消）：");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int id))
                        {
                            if (id == 0)
                            {
                                Console.WriteLine("取消选择");
                                break;
                            }
                            if (character.Skills.FirstOrDefault(s => s.Id == id) is Skill s)
                            {
                                skill = s;
                                break;
                            }
                        }
                    }
                });
            }
            return skill;
        }

        private static async Task GamingQueue_TurnEnd(GamingQueue queue, Character character)
        {
            if (IsRoundHasPlayer_OnlyTest(queue, character))
            {
                // 暂停让玩家查看本回合日志
                Console.WriteLine("你的回合（或与你相关的回合）已结束，请查看本回合日志，然后按任意键继续. . .");
                await Console.In.ReadLineAsync();
            }
            await Task.CompletedTask;
        }

        private static async Task<CharacterActionType> GamingQueue_DecideAction(GamingQueue queue, Character character, List<Character> enemys, List<Character> teammates, List<Skill> skills, List<Item> items)
        {
            CharacterActionType type = CharacterActionType.None;
            if (IsPlayer_OnlyTest(queue, character))
            {
                await Task.Run(() =>
                {
                    Console.WriteLine(character.GetSimpleInfo());
                    while (type == CharacterActionType.None)
                    {
                        Console.Write("现在是你的回合！1：普通攻击，2：使用技能，5：使用物品，6：直接结束回合\r\n请告诉我你的行动：");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int i))
                        {
                            switch ((CharacterActionType)i)
                            {
                                case CharacterActionType.NormalAttack:
                                    type = CharacterActionType.NormalAttack;
                                    break;
                                case CharacterActionType.PreCastSkill:
                                    if (skills.Count == 0)
                                    {
                                        Console.WriteLine("你没有可用的技能，请重新输入！");
                                        break;
                                    }
                                    type = CharacterActionType.PreCastSkill;
                                    break;
                                case CharacterActionType.UseItem:
                                    if (items.Count == 0)
                                    {
                                        Console.WriteLine("你没有可用的物品，请重新输入！");
                                        break;
                                    }
                                    type = CharacterActionType.UseItem;
                                    break;
                                case CharacterActionType.EndTurn:
                                    type = CharacterActionType.EndTurn;
                                    break;
                                default:
                                    Console.WriteLine("输入错误，请重新输入！");
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("输入错误，请重新输入！");
                        }
                    }
                });
            }
            return type;
        }

        private static bool IsPlayer_OnlyTest(GamingQueue queue, Character current)
        {
            return queue.CustomData.TryGetValue("player", out object? value) && value is Character player && player == current;
        }

        private static bool IsRoundHasPlayer_OnlyTest(GamingQueue queue, Character current)
        {
            return queue.CustomData.TryGetValue("player", out object? value) && value is Character player && (player == current || (current.CharacterState != CharacterState.Casting && queue.LastRound.Targets.Any(c => c == player)));
        }

        public static void WriteLine(string str)
        {
            Msg += str + "\r\n";
            if (PrintOut) Console.WriteLine(str);
        }

        public static void DropItems(GamingQueue queue, int mQuality, int wQuality, int aQuality, int sQuality, int acQuality)
        {
            foreach (Character character in queue.Queue)
            {
                Item[] weapons = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("11") && (int)i.QualityType == wQuality)];
                Item[] armors = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("12") && (int)i.QualityType == aQuality)];
                Item[] shoes = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("13") && (int)i.QualityType == sQuality)];
                Item[] accessorys = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("14") && (int)i.QualityType == acQuality)];
                Item? a = null, b = null, c = null, d = null;
                if (weapons.Length > 0)
                {
                    a = weapons[Random.Shared.Next(weapons.Length)];
                }
                if (armors.Length > 0)
                {
                    b = armors[Random.Shared.Next(armors.Length)];
                }
                if (shoes.Length > 0)
                {
                    c = shoes[Random.Shared.Next(shoes.Length)];
                }
                if (accessorys.Length > 0)
                {
                    d = accessorys[Random.Shared.Next(accessorys.Length)];
                }
                List<Item> drops = [];
                if (a != null) drops.Add(a);
                if (b != null) drops.Add(b);
                if (c != null) drops.Add(c);
                if (d != null) drops.Add(d);
                Item? magicCardPack = FunGameService.GenerateMagicCardPack(3, (QualityType)mQuality);
                if (magicCardPack != null)
                {
                    foreach (Skill magic in magicCardPack.Skills.Magics)
                    {
                        magic.Level = 8;
                    }
                    magicCardPack.SetGamingQueue(queue);
                    queue.Equip(character, magicCardPack);
                }
                foreach (Item item in drops)
                {
                    Item realItem = item.Copy();
                    realItem.SetGamingQueue(queue);
                    queue.Equip(character, realItem);
                }
            }
        }

        public static void InitCharacter()
        {
            foreach (Character c in FunGameConstant.Characters)
            {
                CharacterStatistics.Add(c, new());
            }

            StatsConfig.LoadConfig();
            foreach (Character character in CharacterStatistics.Keys)
            {
                if (StatsConfig.ContainsKey(character.ToStringWithOutUser()))
                {
                    CharacterStatistics[character] = StatsConfig.Get<CharacterStatistics>(character.ToStringWithOutUser()) ?? CharacterStatistics[character];
                }
            }
        }

        public static void UpdateStatistics(CharacterStatistics totalStats, CharacterStatistics stats)
        {
            // 统计此角色的所有数据
            totalStats.TotalDamage = Calculation.Round2Digits(totalStats.TotalDamage + stats.TotalDamage);
            totalStats.TotalPhysicalDamage = Calculation.Round2Digits(totalStats.TotalPhysicalDamage + stats.TotalPhysicalDamage);
            totalStats.TotalMagicDamage = Calculation.Round2Digits(totalStats.TotalMagicDamage + stats.TotalMagicDamage);
            totalStats.TotalTrueDamage = Calculation.Round2Digits(totalStats.TotalTrueDamage + stats.TotalTrueDamage);
            totalStats.TotalTakenDamage = Calculation.Round2Digits(totalStats.TotalTakenDamage + stats.TotalTakenDamage);
            totalStats.TotalTakenPhysicalDamage = Calculation.Round2Digits(totalStats.TotalTakenPhysicalDamage + stats.TotalTakenPhysicalDamage);
            totalStats.TotalTakenMagicDamage = Calculation.Round2Digits(totalStats.TotalTakenMagicDamage + stats.TotalTakenMagicDamage);
            totalStats.TotalTakenTrueDamage = Calculation.Round2Digits(totalStats.TotalTakenTrueDamage + stats.TotalTakenTrueDamage);
            totalStats.TotalHeal = Calculation.Round2Digits(totalStats.TotalHeal + stats.TotalHeal);
            totalStats.LiveRound += stats.LiveRound;
            totalStats.ActionTurn += stats.ActionTurn;
            totalStats.LiveTime = Calculation.Round2Digits(totalStats.LiveTime + stats.LiveTime);
            totalStats.ControlTime = Calculation.Round2Digits(totalStats.ControlTime + stats.ControlTime);
            totalStats.TotalEarnedMoney += stats.TotalEarnedMoney;
            totalStats.Kills += stats.Kills;
            totalStats.Deaths += stats.Deaths;
            totalStats.Assists += stats.Assists;
            totalStats.FirstKills += stats.FirstKills;
            totalStats.FirstDeaths += stats.FirstDeaths;
            totalStats.LastRank = stats.LastRank;
            double totalRank = totalStats.AvgRank * totalStats.Plays + totalStats.LastRank;
            double totalRating = totalStats.Rating * totalStats.Plays + stats.Rating;
            totalStats.Plays += stats.Plays;
            if (totalStats.Plays != 0) totalStats.AvgRank = Calculation.Round2Digits(totalRank / totalStats.Plays);
            else totalStats.AvgRank = stats.LastRank;
            if (totalStats.Plays != 0) totalStats.Rating = Calculation.Round4Digits(totalRating / totalStats.Plays);
            else totalStats.Rating = stats.Rating;
            totalStats.Wins += stats.Wins;
            totalStats.Top3s += stats.Top3s;
            totalStats.Loses += stats.Loses;
            totalStats.MVPs += stats.MVPs;
            if (totalStats.Plays != 0)
            {
                totalStats.AvgDamage = Calculation.Round2Digits(totalStats.TotalDamage / totalStats.Plays);
                totalStats.AvgPhysicalDamage = Calculation.Round2Digits(totalStats.TotalPhysicalDamage / totalStats.Plays);
                totalStats.AvgMagicDamage = Calculation.Round2Digits(totalStats.TotalMagicDamage / totalStats.Plays);
                totalStats.AvgTrueDamage = Calculation.Round2Digits(totalStats.TotalTrueDamage / totalStats.Plays);
                totalStats.AvgTakenDamage = Calculation.Round2Digits(totalStats.TotalTakenDamage / totalStats.Plays);
                totalStats.AvgTakenPhysicalDamage = Calculation.Round2Digits(totalStats.TotalTakenPhysicalDamage / totalStats.Plays);
                totalStats.AvgTakenMagicDamage = Calculation.Round2Digits(totalStats.TotalTakenMagicDamage / totalStats.Plays);
                totalStats.AvgTakenTrueDamage = Calculation.Round2Digits(totalStats.TotalTakenTrueDamage / totalStats.Plays);
                totalStats.AvgHeal = Calculation.Round2Digits(totalStats.TotalHeal / totalStats.Plays);
                totalStats.AvgLiveRound = totalStats.LiveRound / totalStats.Plays;
                totalStats.AvgActionTurn = totalStats.ActionTurn / totalStats.Plays;
                totalStats.AvgLiveTime = Calculation.Round2Digits(totalStats.LiveTime / totalStats.Plays);
                totalStats.AvgControlTime = Calculation.Round2Digits(totalStats.ControlTime / totalStats.Plays);
                totalStats.AvgShield = Calculation.Round2Digits(totalStats.TotalShield / totalStats.Plays);
                totalStats.AvgEarnedMoney = totalStats.TotalEarnedMoney / totalStats.Plays;
                totalStats.Winrates = Calculation.Round4Digits(Convert.ToDouble(totalStats.Wins) / Convert.ToDouble(totalStats.Plays));
                totalStats.Top3rates = Calculation.Round4Digits(Convert.ToDouble(totalStats.Top3s) / Convert.ToDouble(totalStats.Plays));
            }
            if (totalStats.LiveRound != 0) totalStats.DamagePerRound = Calculation.Round2Digits(totalStats.TotalDamage / totalStats.LiveRound);
            if (totalStats.ActionTurn != 0) totalStats.DamagePerTurn = Calculation.Round2Digits(totalStats.TotalDamage / totalStats.ActionTurn);
            if (totalStats.LiveTime != 0) totalStats.DamagePerSecond = Calculation.Round2Digits(totalStats.TotalDamage / totalStats.LiveTime);
        }
    }
}
