using System.Text;
using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Oshima.Core.Constant;
using Oshima.FunGame.OshimaModules.Effects.OpenEffects;
using Oshima.FunGame.OshimaModules.Models;
using Oshima.FunGame.OshimaModules.Skills;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    public class GameMapTesting
    {
        public GamingQueue? GamingQueue => _gamingQueue;
        public GameMapController Controller { get; }
        public Dictionary<Character, CharacterStatistics> CharacterStatistics { get; } = [];
        public static Dictionary<Character, CharacterStatistics> TeamCharacterStatistics { get; } = [];
        public PluginConfig StatsConfig { get; } = new(nameof(GameMapTesting), nameof(CharacterStatistics));
        public PluginConfig TeamStatsConfig { get; } = new(nameof(GameMapTesting), nameof(TeamCharacterStatistics));
        public GameMap GameMap { get; } = new TestMap();
        public bool IsWeb { get; set; } = false;
        public bool TeamMode { get; set; } = false;
        public string Msg { get; set; } = "";

        private GamingQueue? _gamingQueue = null;
        private bool _fastMode = false;

        public GameMapTesting(GameMapController controller)
        {
            Controller = controller;
            InitCharacter(); // 初始化角色统计数据
        }

        public async Task<List<string>> StartGame(bool isWeb = false, bool isTeam = false)
        {
            IsWeb = isWeb;
            TeamMode = isTeam;
            try
            {
                List<string> result = [];
                Msg = "";
                List<Character> allCharactersInGame = [.. FunGameConstant.Characters];
                await Controller.WriteLine("--- 游戏开始 ---");
                TeamGamingQueue? tgq = null;
                MixGamingQueue? mgq = null;

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
                    Skill skill = new 疾风步(c)
                    {
                        Level = slevel
                    };
                    c.Skills.Add(skill);
                    foreach (Skill skillLoop in FunGameConstant.CommonPassiveSkills)
                    {
                        Skill passive = skillLoop.Copy();
                        passive.Character = c;
                        passive.Level = 1;
                        c.Skills.Add(passive);
                    }
                    characters.Add(c);
                }

                // 显示角色信息
                foreach (Character c in characters)
                {
                    await Controller.WriteLine($"角色编号：{c.Id}\r\n{c.GetInfo()}");
                }

                // 询问玩家需要选择哪个角色 (通过UI界面选择)
                Character? player = null;
                Character? selectedPlayer = await Controller.RequestCharacterSelection(characters); // 异步等待UI选择
                await Controller.ResolveCharacterSelection(selectedPlayer);
                if (selectedPlayer != null)
                {
                    player = characters.FirstOrDefault(c => c == selectedPlayer);
                    if (player != null)
                    {
                        await Controller.WriteLine($"选择了 [ {player} ]！");
                        player.Promotion = 200;
                        await Controller.SetCurrentCharacter(player);
                        await Controller.SetPlayerCharacter(player);
                    }
                }

                if (player is null)
                {
                    throw new Exception("没有选择角色，游戏结束。");
                }

                // 创建顺序表并排序
                Team? team1 = null, team2 = null;
                if (isTeam)
                {
                    tgq = new(WriteLine)
                    {
                        GameplayEquilibriumConstant = OshimaGameModuleConstant.GameplayEquilibriumConstant
                    };
                    _gamingQueue = tgq;
                    await Controller.WriteLine("=== 团队模式随机分组 ===");

                    // 打乱角色列表
                    List<Character> shuffledCharacters = [.. characters.OrderBy(c => Random.Shared.Next())];

                    // 创建两个团队
                    List<Character> group1 = [];
                    List<Character> group2 = [];

                    // 将角色交替分配到两个团队中
                    for (int cid = 0; cid < shuffledCharacters.Count; cid++)
                    {
                        Character thisCharacter = shuffledCharacters[cid];
                        if (cid % 2 == 0)
                        {
                            group1.Add(thisCharacter);
                        }
                        else
                        {
                            group2.Add(thisCharacter);
                        }
                    }

                    // 添加到团队字典，第一个为队长
                    tgq.AddTeam($"{group1.First().NickName}的小队", group1);
                    tgq.AddTeam($"{group2.First().NickName}的小队", group2);

                    foreach (Team team in tgq.Teams.Values)
                    {
                        await Controller.WriteLine($"团队【{team.Name}】的成员：\r\n{string.Join("\r\n", team.Members)}\r\n");
                        if (team.IsOnThisTeam(player))
                        {
                            foreach (Character c in team.Members.Except([player]))
                            {
                                c.Promotion = 300;
                            }
                            team1 = team;
                        }
                        else
                        {
                            team.Members.ForEach(c => c.Promotion = 400);
                            team2 = team;
                        }
                    }

                    // 初始化角色
                    _gamingQueue.InitCharacters(characters);
                }
                else
                {
                    mgq = new MixGamingQueue(characters, WriteLine)
                    {
                        GameplayEquilibriumConstant = OshimaGameModuleConstant.GameplayEquilibriumConstant
                    };
                    _gamingQueue = mgq;
                }
                if (team1 != null && team2 != null) await Controller.SetTeamCharacters(team1.Members, team2.Members);

                // 加载地图和绑定事件
                _gamingQueue.LoadGameMap(GameMap);
                if (_gamingQueue.Map != null)
                {
                    GameMap map = _gamingQueue.Map;
                    // 随机放置角色
                    HashSet<Grid> allocated = [];
                    List<Grid> grids = [.. map.Grids.Values];
                    foreach (Character character in characters)
                    {
                        character.NormalAttack.GamingQueue = _gamingQueue;
                        Grid grid = Grid.Empty;
                        do
                        {
                            grid = grids[Random.Shared.Next(grids.Count)];
                        }
                        while (allocated.Contains(grid));
                        allocated.Add(grid);
                        map.SetCharacterCurrentGrid(character, grid);
                    }
                    await Controller.SetGameMap(map);
                    _gamingQueue.SelectTargetGrid += GamingQueue_SelectTargetGrid;
                    _gamingQueue.CharacterMove += GamingQueue_CharacterMove;
                }

                // 绑定事件
                await Controller.SetQueue(_gamingQueue.HardnessTime);
                await Controller.SetCharacterStatistics(_gamingQueue.CharacterStatistics);
                _gamingQueue.TurnStart += GamingQueue_TurnStart;
                _gamingQueue.DecideAction += GamingQueue_DecideAction;
                _gamingQueue.SelectNormalAttackTargets += GamingQueue_SelectNormalAttackTargets;
                _gamingQueue.SelectSkill += GamingQueue_SelectSkill;
                _gamingQueue.SelectSkillTargets += GamingQueue_SelectSkillTargets;
                _gamingQueue.SelectItem += GamingQueue_SelectItem;
                _gamingQueue.QueueUpdated += GamingQueue_QueueUpdated;
                _gamingQueue.TurnEnd += GamingQueue_TurnEnd;

                // 总游戏时长
                double totalTime = 0;

                // 开始空投
                Msg = "";
                int qMagicCardPack = 5;
                int qWeapon = 5;
                int qArmor = 5;
                int qShoes = 5;
                int qAccessory = 5;
                await Controller.WriteLine($"社区送温暖了，现在随机发放空投！！");
                DropItems(_gamingQueue, qMagicCardPack, qWeapon, qArmor, qShoes, qAccessory);
                await Controller.WriteLine("");
                if (isWeb) result.Add("=== 空投 ===\r\n" + Msg);
                double nextDropItemTime = 40;
                if (qMagicCardPack < 5) qMagicCardPack++;
                if (qWeapon < 5) qWeapon++;
                if (qArmor < 5) qArmor++;
                if (qShoes < 5) qShoes++;
                if (qAccessory < 5) qAccessory++;

                // 显示角色信息
                foreach (Character c in characters)
                {
                    await Controller.WriteLine(c.GetInfo());
                }

                // 初始化队列，准备开始游戏
                _gamingQueue.InitActionQueue();
                _gamingQueue.SetCharactersToAIControl(false, characters);
                _gamingQueue.SetCharactersToAIControl(true, player);
                _gamingQueue.CustomData.Add("player", player);
                await Controller.WriteLine();

                // 显示初始顺序表
                _gamingQueue.DisplayQueue();
                await Controller.WriteLine();

                await Controller.WriteLine($"你的角色是 [ {player} ]，详细信息：{player.GetInfo()}");

                // 总回合数
                int maxRound = isTeam ? 9999 : 999;

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
                _gamingQueue.InitRoundRewards(maxRound, 1, effects, id => FunGameConstant.RoundRewards[(EffectID)id]);
                await Controller.SetTurnRewards(_gamingQueue.RoundRewards);

                int i = 1;
                while (i < maxRound)
                {
                    Msg = "";
                    if (i == maxRound - 1)
                    {
                        if (isTeam)
                        {
                            WriteLine("两队打到天昏地暗，流局了！！");
                            break;
                        }
                        else
                        {
                            await Controller.WriteLine($"=== 终局审判 ===");
                            Dictionary<Character, double> hpPercentage = [];
                            foreach (Character c in characters)
                            {
                                hpPercentage.TryAdd(c, c.HP / c.MaxHP);
                            }
                            double max = hpPercentage.Values.Max();
                            Character winner = hpPercentage.Keys.Where(c => hpPercentage[c] == max).First();
                            await Controller.WriteLine("[ " + winner + " ] 成为了天选之人！！");
                            foreach (Character c in characters.Where(c => c != winner && c.HP > 0))
                            {
                                await Controller.WriteLine("[ " + winner + " ] 对 [ " + c + " ] 造成了 99999999999 点真实伤害。");
                                await _gamingQueue.DeathCalculationAsync(winner, c);
                            }
                            if (mgq != null)
                            {
                                await mgq.EndGameInfo(winner);
                            }
                        }
                        result.Add(Msg);
                        break;
                    }

                    // 检查是否有角色可以行动
                    Character? characterToAct = await _gamingQueue.NextCharacterAsync();
                    await Controller.UpdateQueue();
                    await Controller.UpdateCharacterPositionsOnMap();

                    // 处理回合
                    if (characterToAct != null)
                    {
                        await Controller.SetCurrentRound(i);
                        await Controller.WriteLine($"=== 回合 {i++} ===");
                        await Controller.WriteLine("现在是 [ " + characterToAct + " ] 的回合！");

                        // 获取该角色当前位置
                        if (_gamingQueue.Map != null)
                        {
                            Grid? currentGrid = _gamingQueue.Map.GetCharacterCurrentGrid(characterToAct);
                            if (currentGrid != null) _gamingQueue.CustomData["currentGrid"] = currentGrid;
                        }

                        bool isGameEnd = await _gamingQueue.ProcessTurnAsync(characterToAct);

                        if (isGameEnd)
                        {
                            result.Add(Msg);
                            break;
                        }

                        if (isWeb) _gamingQueue.DisplayQueue();
                        await Controller.WriteLine("");
                    }

                    string roundMsg = "";
                    if (_gamingQueue.LastRound.HasKill)
                    {
                        roundMsg = Msg;
                        Msg = "";
                    }

                    // 模拟时间流逝
                    double timeLapse = await _gamingQueue.TimeLapse();
                    totalTime += timeLapse;
                    nextDropItemTime -= timeLapse;
                    await Controller.UpdateBottomInfoPanel();
                    await Controller.UpdateQueue();
                    await Controller.UpdateCharacterPositionsOnMap();
                    if (team1 != null && team2 != null) await Controller.SetTeamCharacters(team1.Members, team2.Members);

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
                        await Controller.WriteLine($"社区送温暖了，现在随机发放空投！！");
                        DropItems(_gamingQueue, qMagicCardPack, qWeapon, qArmor, qShoes, qAccessory);
                        await Controller.WriteLine("");
                        if (isWeb) result.Add("=== 空投 ===\r\n" + Msg);
                        nextDropItemTime = 40;
                        if (qMagicCardPack < 5) qMagicCardPack++;
                        if (qWeapon < 5) qWeapon++;
                        if (qArmor < 5) qArmor++;
                        if (qShoes < 5) qShoes++;
                        if (qAccessory < 5) qAccessory++;
                    }
                }

                await Controller.WriteLine("--- 游戏结束 ---");
                await Controller.WriteLine($"总游戏时长：{totalTime:0.##} {_gamingQueue.GameplayEquilibriumConstant.InGameTime}");

                // 赛后统计
                FunGameService.GetCharacterRating(_gamingQueue.CharacterStatistics, isTeam, tgq != null ? tgq.EliminatedTeams : []);

                // 统计技术得分，评选 MVP
                Character? mvp = _gamingQueue.CharacterStatistics.OrderByDescending(d => d.Value.Rating).Select(d => d.Key).FirstOrDefault();
                StringBuilder mvpBuilder = new();
                if (mvp != null)
                {
                    CharacterStatistics stats = _gamingQueue.CharacterStatistics[mvp];
                    stats.MVPs++;
                    mvpBuilder.AppendLine($"{(tgq != null ? "[ " + tgq.GetTeamFromEliminated(mvp)?.Name + " ] " : "")}[ {mvp.ToStringWithLevel()} ]");
                    mvpBuilder.AppendLine($"技术得分：{stats.Rating:0.0#} / 击杀数：{stats.Kills} / 助攻数：{stats.Assists}{(_gamingQueue.MaxRespawnTimes != 0 ? " / 死亡数：" + stats.Deaths : "")}");
                    mvpBuilder.AppendLine($"存活时长：{stats.LiveTime:0.##} / 存活回合数：{stats.LiveRound} / 行动回合数：{stats.ActionTurn}");
                    mvpBuilder.AppendLine($"控制时长：{stats.ControlTime:0.##} / 总计治疗：{stats.TotalHeal:0.##} / 护盾抵消：{stats.TotalShield:0.##}");
                    mvpBuilder.AppendLine($"总计伤害：{stats.TotalDamage:0.##} / 总计物理伤害：{stats.TotalPhysicalDamage:0.##} / 总计魔法伤害：{stats.TotalMagicDamage:0.##}");
                    mvpBuilder.AppendLine($"总承受伤害：{stats.TotalTakenDamage:0.##} / 总承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 总承受魔法伤害：{stats.TotalTakenMagicDamage:0.##}");
                    if (stats.TotalTrueDamage > 0 || stats.TotalTakenTrueDamage > 0) mvpBuilder.AppendLine($"总计真实伤害：{stats.TotalTrueDamage:0.##} / 总承受真实伤害：{stats.TotalTakenTrueDamage:0.##}");
                    mvpBuilder.Append($"每秒伤害：{stats.DamagePerSecond:0.##} / 每回合伤害：{stats.DamagePerTurn:0.##}");
                }

                int top = isWeb ? _gamingQueue.CharacterStatistics.Count : 0;
                int count = 1;
                if (isWeb)
                {
                    await Controller.WriteLine("=== 技术得分排行榜 ===");
                    Msg = $"=== 技术得分排行榜 TOP{top} ===\r\n";
                }
                else
                {
                    StringBuilder ratingBuilder = new();
                    await Controller.WriteLine("=== 本场比赛最佳角色 ===");
                    Msg = $"=== 本场比赛最佳角色 ===\r\n";
                    await Controller.WriteLine(mvpBuilder.ToString() + "\r\n\r\n" + ratingBuilder.ToString());

                    await Controller.WriteLine();
                    await Controller.WriteLine("=== 技术得分排行榜 ===");
                }

                foreach (Character character in _gamingQueue.CharacterStatistics.OrderByDescending(d => d.Value.Rating).Select(d => d.Key))
                {
                    StringBuilder builder = new();
                    CharacterStatistics stats = _gamingQueue.CharacterStatistics[character];
                    builder.AppendLine($"{(isWeb ? count + ". " : "")}[ {character.ToStringWithLevel()}{(tgq != null ? "（" + tgq.GetTeamFromEliminated(character)?.Name + "）" : "")} ]");
                    builder.AppendLine($"技术得分：{stats.Rating:0.0#} / 击杀数：{stats.Kills} / 助攻数：{stats.Assists}{(_gamingQueue.MaxRespawnTimes != 0 ? " / 死亡数：" + stats.Deaths : "")}");
                    builder.AppendLine($"存活时长：{stats.LiveTime:0.##} / 存活回合数：{stats.LiveRound} / 行动回合数：{stats.ActionTurn}");
                    builder.AppendLine($"控制时长：{stats.ControlTime:0.##} / 总计治疗：{stats.TotalHeal:0.##} / 护盾抵消：{stats.TotalShield:0.##}");
                    builder.AppendLine($"总计伤害：{stats.TotalDamage:0.##} / 总计物理伤害：{stats.TotalPhysicalDamage:0.##} / 总计魔法伤害：{stats.TotalMagicDamage:0.##}");
                    builder.AppendLine($"总承受伤害：{stats.TotalTakenDamage:0.##} / 总承受物理伤害：{stats.TotalTakenPhysicalDamage:0.##} / 总承受魔法伤害：{stats.TotalTakenMagicDamage:0.##}");
                    if (stats.TotalTrueDamage > 0 || stats.TotalTakenTrueDamage > 0) builder.AppendLine($"总计真实伤害：{stats.TotalTrueDamage:0.##} / 总承受真实伤害：{stats.TotalTakenTrueDamage:0.##}");
                    builder.Append($"每秒伤害：{stats.DamagePerSecond:0.##} / 每回合伤害：{stats.DamagePerTurn:0.##}");
                    if (count++ <= top)
                    {
                        await Controller.WriteLine(builder.ToString());
                    }
                    else
                    {
                        await Controller.WriteLine(builder.ToString());
                    }

                    CharacterStatistics? totalStats = isTeam ?
                        TeamCharacterStatistics.Where(kv => kv.Key.GetName() == character.GetName()).Select(kv => kv.Value).FirstOrDefault() : 
                        CharacterStatistics.Where(kv => kv.Key.GetName() == character.GetName()).Select(kv => kv.Value).FirstOrDefault();
                    if (totalStats != null)
                    {
                        UpdateStatistics(totalStats, stats);
                    }
                }
                result.Add(Msg);

                if (isWeb)
                {
                    for (i = _gamingQueue.Eliminated.Count - 1; i >= 0; i--)
                    {
                        Character character = _gamingQueue.Eliminated[i];
                        result.Add($"=== 角色 [ {character} ] ===\r\n{character.GetInfo()}");
                    }
                }

                if (isTeam)
                {
                    lock (TeamStatsConfig)
                    {
                        foreach (Character c in TeamCharacterStatistics.Keys)
                        {
                            TeamStatsConfig.Add(c.ToStringWithOutUser(), TeamCharacterStatistics[c]);
                        }
                        TeamStatsConfig.SaveConfig();
                    }
                }
                else
                {
                    lock (StatsConfig)
                    {
                        foreach (Character c in CharacterStatistics.Keys)
                        {
                            StatsConfig.Add(c.ToStringWithOutUser(), CharacterStatistics[c]);
                        }
                        StatsConfig.SaveConfig();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await Controller.WriteLine(ex.ToString());
                return [ex.ToString()];
            }
        }

        private async Task<Grid> GamingQueue_SelectTargetGrid(GamingQueue queue, Character character, List<Character> enemys, List<Character> teammates, GameMap map, List<Grid> moveRange)
        {
            if (!IsPlayer_OnlyTest(queue, character) || _gamingQueue is null || _gamingQueue.Map is null) return Grid.Empty;

            Grid current =  Grid.Empty;
            if (_gamingQueue.CustomData.TryGetValue("currentGrid", out object? currentGrid) && currentGrid is Grid grid)
            {
                current = grid;
            }
            if (current == Grid.Empty)
            {
                return current;
            }

            // 通过UI请求目标选择
            Grid? selectedTargetGrid = await Controller.RequestTargetGridSelection(
                character,
                current,
                _gamingQueue.Map.GetGridsByRange(current, character.MOV)
            );
            await Controller.ResolveTargetGridSelection(selectedTargetGrid);

            return selectedTargetGrid ?? Grid.Empty;
        }

        private async Task GamingQueue_CharacterMove(GamingQueue queue, Character actor, Grid grid)
        {
            await Controller.UpdateCharacterPositionsOnMap();
        }

        private async Task GamingQueue_QueueUpdated(GamingQueue queue, List<Character> characters, Character character, double hardnessTime, QueueUpdatedReason reason, string msg)
        {
            if (reason != QueueUpdatedReason.Action)
            {
                await Controller.UpdateQueue();
            }
        }

        private async Task<bool> GamingQueue_TurnStart(GamingQueue queue, Character character, List<Character> enemys, List<Character> teammates, List<Skill> skills, List<Item> items)
        {
            await Controller.UpdateBottomInfoPanel();
            return true;
        }

        private async Task<List<Character>> GamingQueue_SelectNormalAttackTargets(GamingQueue queue, Character character, NormalAttack attack, List<Character> enemys, List<Character> teammates, List<Grid> attackRange)
        {
            if (!IsPlayer_OnlyTest(queue, character)) return [];

            // 通过UI请求目标选择
            List<Character> selectedTargets = await Controller.RequestTargetSelection(
                character,
                attack,
                enemys,
                teammates,
                attackRange
            );
            await Controller.ResolveTargetSelection(selectedTargets);

            return selectedTargets ?? []; // 如果取消，返回空列表
        }

        private async Task<Item?> GamingQueue_SelectItem(GamingQueue queue, Character character, List<Item> items)
        {
            if (!IsPlayer_OnlyTest(queue, character)) return null;

            // 通过UI请求物品选择
            Item? selectedItem = await Controller.RequestItemSelection(character, items);
            await Controller.ResolveItemSelection(selectedItem);
            return selectedItem;
        }

        private async Task<List<Character>> GamingQueue_SelectSkillTargets(GamingQueue queue, Character caster, Skill skill, List<Character> enemys, List<Character> teammates, List<Grid> castRange)
        {
            if (!IsPlayer_OnlyTest(queue, caster)) return [];

            List<Character> potentialTargets = [];
            if (skill.CanSelectEnemy) potentialTargets.AddRange(enemys);
            if (skill.CanSelectTeammate) potentialTargets.AddRange(teammates);
            if (skill.CanSelectSelf) potentialTargets.Add(caster);

            // 通过UI请求目标选择
            List<Character>? selectedTargets = await Controller.RequestTargetSelection(
                caster,
                skill,
                enemys,
                teammates,
                castRange
            );
            await Controller.ResolveTargetSelection(selectedTargets);

            return selectedTargets ?? []; // 如果取消，返回空列表
        }

        private async Task<Skill?> GamingQueue_SelectSkill(GamingQueue queue, Character character, List<Skill> skills)
        {
            if (!IsPlayer_OnlyTest(queue, character)) return null;

            // 通过UI请求技能选择
            Skill? selectedSkill = await Controller.RequestSkillSelection(character, skills);
            await Controller.ResolveSkillSelection(selectedSkill);
            return selectedSkill;
        }

        private async Task GamingQueue_TurnEnd(GamingQueue queue, Character character)
        {
            double ht = queue.HardnessTime[character];
            await Controller.SetPredictCharacter(character.NickName, ht);
            await Controller.UpdateBottomInfoPanel();
            if (!_fastMode)
            {
                if (IsRoundHasPlayer_OnlyTest(queue, character))
                {
                    // 玩家回合结束，等待玩家确认
                    await Controller.RequestContinuePrompt("你的回合（或与你相关的回合）已结束，请查看本回合日志，然后点击继续. . .");
                    await Controller.ResolveContinuePrompt();
                }
                else
                {
                    await Controller.RequestCountDownContinuePrompt("该角色的回合已结束. . .");
                    await Controller.ResolveCountDownContinuePrompt();
                }
            }
            else await Task.Delay(100);
        }

        private async Task<CharacterActionType> GamingQueue_DecideAction(GamingQueue queue, Character character, List<Character> enemys, List<Character> teammates, List<Skill> skills, List<Item> items)
        {
            if (IsPlayer_OnlyTest(queue, character))
            {
                // 通过UI按钮请求行动类型
                CharacterActionType actionType = await Controller.RequestActionType(character, items);
                await Controller.ResolveActionType(actionType);
                return actionType;
            }
            return CharacterActionType.None; // 非玩家角色，由AI处理，或默认None
        }

        private static bool IsPlayer_OnlyTest(GamingQueue queue, Character current)
        {
            return queue.CustomData.TryGetValue("player", out object? value) && value is Character player && player == current && !queue.IsCharacterInAIControlling(current);
        }

        private static bool IsRoundHasPlayer_OnlyTest(GamingQueue queue, Character current)
        {
            return queue.CustomData.TryGetValue("player", out object? value) && value is Character player && (player == current || (current.CharacterState != CharacterState.Casting && queue.LastRound.Targets.Any(c => c == player)));
        }

        public async Task SetPreCastSuperSkill(Character character, Skill skill)
        {
            if (_gamingQueue is GamingQueue queue)
            {
                await queue.SetCharacterPreCastSuperSkill(character, skill);
            }
        }

        public void SetAutoMode(bool cancel, Character character)
        {
            _gamingQueue?.SetCharactersToAIControl(cancel, character);
        }

        public void SetFastMode(bool on)
        {
            _fastMode = on;
        }

        public void WriteLine(string str)
        {
            Msg += str + "\r\n";
            _ = Controller.WriteLine(str);
        }

        public static void DropItems(GamingQueue queue, int mQuality, int wQuality, int aQuality, int sQuality, int acQuality)
        {
            Item[] weapons = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("11") && (int)i.QualityType == wQuality)];
            Item[] armors = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("12") && (int)i.QualityType == aQuality)];
            Item[] shoes = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("13") && (int)i.QualityType == sQuality)];
            Item[] accessorys = [.. FunGameConstant.Equipment.Where(i => i.Id.ToString().StartsWith("14") && (int)i.QualityType == acQuality)];
            Item[] consumables = [.. FunGameConstant.AllItems.Where(i => i.ItemType == ItemType.Consumable && i.IsInGameItem)];
            foreach (Character character in queue.AllCharacters)
            {
                Item? a = null, b = null, c = null, d = null;
                if (weapons.Length > 0) a = weapons[Random.Shared.Next(weapons.Length)];
                if (armors.Length > 0) b = armors[Random.Shared.Next(armors.Length)];
                if (shoes.Length > 0) c = shoes[Random.Shared.Next(shoes.Length)];
                if (accessorys.Length > 0) d = accessorys[Random.Shared.Next(accessorys.Length)];
                List<Item> drops = [];
                if (a != null) drops.Add(a);
                if (b != null) drops.Add(b);
                if (c != null) drops.Add(c);
                if (d != null) drops.Add(d);
                mQuality = 2;
                Item? magicCardPack = FunGameService.GenerateMagicCardPack(8, (QualityType)mQuality);
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
                if (consumables.Length > 0 && character.Items.Count < 5)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Item consumable = consumables[Random.Shared.Next(consumables.Length)].Copy();
                        consumable.Character = character;
                        character.Items.Add(consumable);
                    }
                }
            }
        }

        public void InitCharacter()
        {
            foreach (Character c in FunGameConstant.Characters)
            {
                CharacterStatistics.Add(c, new());
                TeamCharacterStatistics.Add(c, new());
            }

            StatsConfig.LoadConfig();
            TeamStatsConfig.LoadConfig();
            foreach (Character character in CharacterStatistics.Keys)
            {
                if (StatsConfig.ContainsKey(character.ToStringWithOutUser()))
                {
                    CharacterStatistics[character] = StatsConfig.Get<CharacterStatistics>(character.ToStringWithOutUser()) ?? CharacterStatistics[character];
                }
            }
            foreach (Character character in TeamCharacterStatistics.Keys)
            {
                if (TeamStatsConfig.ContainsKey(character.ToStringWithOutUser()))
                {
                    TeamCharacterStatistics[character] = TeamStatsConfig.Get<CharacterStatistics>(character.ToStringWithOutUser()) ?? TeamCharacterStatistics[character];
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
            totalStats.TotalShield = Calculation.Round2Digits(totalStats.TotalShield + stats.TotalShield);
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

        public List<Team> GetTeams()
        {
            if (_gamingQueue is TeamGamingQueue tgq)
            {
                return [.. tgq.Teams.Values];
            }
            return [];
        }
    }
}
