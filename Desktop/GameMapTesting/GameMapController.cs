using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    public class GameMapController(GameMapViewer ui)
    {
        public GameMapViewer UI => ui;
        private GameMapTesting? _game;

        public bool TeamMode => _game?.TeamMode ?? false;

        // 输入请求器实例
        private readonly UserInputRequester<Character> _characterSelectionRequester = new();
        private readonly UserInputRequester<Grid> _targetGridSelectionRequester = new();
        private readonly UserInputRequester<CharacterActionType> _actionTypeRequester = new();
        private readonly UserInputRequester<List<Character>> _targetSelectionRequester = new();
        private readonly UserInputRequester<Skill> _skillSelectionRequester = new();
        private readonly UserInputRequester<Item> _itemSelectionRequester = new();
        private readonly UserInputRequester<bool> _continuePromptRequester = new(); // 用于“按任意键继续”提示

        public async Task WriteLine(string str = "") => await UI.AppendDebugLog(str);

        public async Task Start()
        {
            _game = new(this);
            await _game.StartGame(false, true);
        }

        public List<Team> GetTeams()
        {
            return _game?.GetTeams() ?? [];
        }

        public async Task SetTeamCharacters(IEnumerable<Character> teammates, IEnumerable<Character> enemies)
        {
            await UI.InvokeAsync(() =>
            {
                UI.TeammateCharacters.Clear();
                foreach (Character character in teammates)
                {
                    UI.TeammateCharacters.Add(new CharacterViewModel(character));
                }
                UI.EnemyCharacters.Clear();
                foreach (Character character in enemies)
                {
                    UI.EnemyCharacters.Add(new CharacterViewModel(character));
                }
            });
        }

        public async Task SetPreCastSuperSkill(Character character, Skill skill)
        {
            if (_game != null)
            {
                await _game.SetPreCastSuperSkill(character, skill);
            }
        }

        public void SetAutoMode(bool cancel, Character character)
        {
            _game?.SetAutoMode(cancel, character);
        }

        public void SetFastMode(bool on)
        {
            _game?.SetFastMode(on);
        }

        public async Task SetPredictCharacter(string name, double ht)
        {
            await UI.InvokeAsync(() => UI.SetPredictCharacter(name, ht));
        }

        public async Task<Character?> RequestCharacterSelection(List<Character> availableCharacters)
        {
            await WriteLine("请选择你想玩的角色。");
            return await _characterSelectionRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowCharacterSelectionPrompt(availableCharacters, callback))
            );
        }

        public async Task<CharacterActionType> RequestActionType(Character character, List<Item> availableItems)
        {
            await WriteLine($"现在是 {character.NickName} 的回合，请选择行动。");
            return await _actionTypeRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowActionButtons(character, availableItems, callback))
            );
        }

        public async Task<Grid?> RequestTargetGridSelection(Character character, Grid currentGrid, List<Grid> selectable)
        {
            await WriteLine($"请选择一个目标地点。");
            return await _targetGridSelectionRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowTargetGridSelectionUI(character, currentGrid, selectable, callback))
            );
        }

        public async Task<List<Character>> RequestTargetSelection(Character character, ISkill skill, List<Character> enemys, List<Character> teammates)
        {
            List<Character> selectable = skill.GetSelectableTargets(character, enemys, teammates);
            await WriteLine($"请为 {character.NickName} 选择目标 (最多 {skill.RealCanSelectTargetCount(enemys, teammates)} 个)。");
            List<Character> targetIds = await _targetSelectionRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowTargetSelectionUI(character, skill, selectable, enemys, teammates, callback))
            ) ?? [];
            if (targetIds == null) return [];
            return [.. selectable.Where(targetIds.Contains)];
        }

        public async Task<Skill?> RequestSkillSelection(Character character, List<Skill> availableSkills)
        {
            await WriteLine($"请为 {character.NickName} 选择一个技能。");
            Skill? skill = await _skillSelectionRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowSkillSelectionUI(character, callback))
            );
            return availableSkills.Any(s => s == skill) ? skill : null;
        }

        public async Task<Item?> RequestItemSelection(Character character, List<Item> availableItems)
        {
            await WriteLine($"请为 {character.NickName} 选择一个物品。");
            Item? item = await _itemSelectionRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowItemSelectionUI(character, callback))
            );
            return availableItems.Any(i => i == item) ? item : null;
        }

        public async Task RequestContinuePrompt(string message)
        {
            await WriteLine(message);
            await _continuePromptRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(() => UI.ShowContinuePrompt(message, callback))
            );
        }

        public async Task RequestCountDownContinuePrompt(string message, int countdownSeconds = 2)
        {
            await WriteLine(message);
            // 调用 _continuePromptRequester 的 RequestInput 方法，它会等待回调被触发
            await _continuePromptRequester.RequestInput(
                async (callback) => await UI.InvokeAsync(async () => await UI.StartCountdownForContinue(countdownSeconds, callback))
            );
        }

        // --- GameMapViewer 调用这些方法来解决 UI 输入 ---

        public async Task ResolveCharacterSelection(Character? character)
        {
            _characterSelectionRequester.ResolveInput(character);
            await UI.InvokeAsync(() => UI.HideCharacterSelectionPrompt());
        }

        public async Task ResolveActionType(CharacterActionType actionType)
        {
            _actionTypeRequester.ResolveInput(actionType);
            await UI.InvokeAsync(() => UI.HideActionButtons());
        }

        public async Task ResolveTargetSelection(List<Character> targetIds)
        {
            _targetSelectionRequester.ResolveInput(targetIds);
            await UI.InvokeAsync(() => UI.HideTargetSelectionUI());
        }

        public async Task ResolveTargetGridSelection(Grid? grid)
        {
            _targetGridSelectionRequester.ResolveInput(grid);
            await UI.InvokeAsync(() => UI.HideTargetGridSelectionUI());
        }

        public async Task ResolveSkillSelection(Skill? skill)
        {
            _skillSelectionRequester.ResolveInput(skill);
            await UI.InvokeAsync(() => UI.HideSkillSelectionUI());
        }

        public async Task ResolveItemSelection(Item? item)
        {
            _itemSelectionRequester.ResolveInput(item);
            await UI.InvokeAsync(() => UI.HideItemSelectionUI());
        }

        public async Task ResolveContinuePrompt()
        {
            _continuePromptRequester.ResolveInput(true); // 任何值都可以，只要完成Task
            await UI.InvokeAsync(() => UI.HideContinuePrompt());
        }
        
        public async Task ResolveCountDownContinuePrompt()
        {
            _continuePromptRequester.ResolveInput(true);
            await UI.InvokeAsync(() => UI.HideContinuePrompt());
        }

        public bool IsTeammate(Character actor, Character target)
        {
            if (actor == target) return true;
            if (_game != null && _game.GamingQueue != null)
            {
                return _game.GamingQueue.IsTeammate(actor, target);
            }
            return false;
        }

        public async Task UpdateBottomInfoPanel()
        {
            await UI.InvokeAsync(UI.UpdateBottomInfoPanel);
        }
        
        public async Task UpdateQueue()
        {
            await UI.InvokeAsync(UI.UpdateLeftQueuePanelGrid);
        }
        
        public async Task UpdateCharacterPositionsOnMap()
        {
            await UI.InvokeAsync(UI.UpdateCharacterPositionsOnMap);
        }

        public async Task SetQueue(Dictionary<Character, double> dict)
        {
            await UI.InvokeAsync(() =>
            {
                UI.CharacterQueueData = dict;
            });
        }

        public async Task SetGameMap(GameMap map)
        {
            await UI.InvokeAsync(() =>
            {
                UI.CurrentGameMap = map;
            });
        }
        
        public async Task SetCurrentRound(int round)
        {
            await UI.InvokeAsync(() =>
            {
                UI.CurrentRound = round;
            });
        }
        
        public async Task SetTurnRewards(Dictionary<int, List<Skill>> rewards)
        {
            await UI.InvokeAsync(() =>
            {
                UI.TurnRewards = rewards;
            });
        }

        public async Task SetPlayerCharacter(Character character)
        {
            await UI.InvokeAsync(() =>
            {
                UI.PlayerCharacter = character;
            });
        }
        
        public async Task SetCurrentCharacter(Character character)
        {
            await UI.InvokeAsync(() =>
            {
                UI.CurrentCharacter = character;
            });
        }

        public async Task SetCharacterStatistics(Dictionary<Character, CharacterStatistics> stats)
        {
            await UI.InvokeAsync(() =>
            {
                UI.CharacterStatistics = stats;
            });
        }
    }
    
    /// <summary>
     /// 辅助类，用于管理异步的用户输入请求。
     /// </summary>
     /// <typeparam name="T">期望的用户输入类型。</typeparam>
    public class UserInputRequester<T>
    {
        private TaskCompletionSource<T?>? _tcs;

        /// <summary>
        /// 请求用户输入，并等待结果。
        /// </summary>
        /// <param name="uiPromptAction">一个Action，用于通知UI显示提示，并传入一个回调函数供UI完成输入后调用。</param>
        /// <returns>用户输入的结果，如果用户取消则为null。</returns>
        public async Task<T?> RequestInput(Action<Action<T?>> uiPromptAction)
        {
            _tcs = new TaskCompletionSource<T?>();
            // 调用UI动作，并传入我们的ResolveInput方法作为回调
            // UI将在获取输入后调用此回调
            uiPromptAction(ResolveInput);
            return await _tcs.Task;
        }

        /// <summary>
        /// 解决用户输入请求，将结果传递给等待的Task。
        /// </summary>
        /// <param name="result">用户输入的结果。</param>
        public void ResolveInput(T? result)
        {
            _tcs?.TrySetResult(result);
            _tcs = null; // 清除以防止意外重用
        }
    }
}
