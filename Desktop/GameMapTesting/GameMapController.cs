using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Common.Addon;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    public class GameMapController(GameMapViewer ui)
    {
        public GameMapViewer UI => ui;
        private GameMapTesting? _game;

        // 输入请求器实例
        private readonly UserInputRequester<long> _characterSelectionRequester = new();
        private readonly UserInputRequester<CharacterActionType> _actionTypeRequester = new();
        private readonly UserInputRequester<List<Character>> _targetSelectionRequester = new();
        private readonly UserInputRequester<long> _skillSelectionRequester = new();
        private readonly UserInputRequester<long> _itemSelectionRequester = new();
        private readonly UserInputRequester<bool> _continuePromptRequester = new(); // 用于“按任意键继续”提示

        public void WriteLine(string str = "") => UI.AppendDebugLog(str);

        public async Task Start()
        {
            _game = new(this);
            await _game.StartGame(false);
        }

        public async Task<long> RequestCharacterSelection(List<Character> availableCharacters)
        {
            WriteLine("请选择你想玩的角色。");
            return await _characterSelectionRequester.RequestInput(
                (callback) => UI.Invoke(() => UI.ShowCharacterSelectionPrompt(availableCharacters, callback))
            );
        }

        public async Task<CharacterActionType> RequestActionType(Character character, List<Skill> availableSkills, List<Item> availableItems)
        {
            WriteLine($"现在是 {character.NickName} 的回合，请选择行动。");
            return await _actionTypeRequester.RequestInput(
                (callback) => UI.Invoke(() => UI.ShowActionButtons(character, availableSkills, availableItems, callback))
            );
        }

        public async Task<List<Character>> RequestTargetSelection(Character actor, List<Character> potentialTargets, long maxTargets, bool canSelectSelf, bool canSelectEnemy, bool canSelectTeammate)
        {
            WriteLine($"请为 {actor.NickName} 选择目标 (最多 {maxTargets} 个)。");
            List<Character> targetIds = await _targetSelectionRequester.RequestInput(
                (callback) => UI.Invoke(() => UI.ShowTargetSelectionUI(actor, potentialTargets, maxTargets, canSelectSelf, canSelectEnemy, canSelectTeammate, callback))
            ) ?? [];
            if (targetIds == null) return [];
            return [.. potentialTargets.Where(targetIds.Contains)];
        }

        public async Task<Skill?> RequestSkillSelection(Character character, List<Skill> availableSkills)
        {
            WriteLine($"请为 {character.NickName} 选择一个技能。");
            long? skillId = await _skillSelectionRequester.RequestInput(
                (callback) => UI.Invoke(() => UI.ShowSkillSelectionUI(character, availableSkills, callback))
            );
            return skillId.HasValue ? availableSkills.FirstOrDefault(s => s.Id == skillId.Value) : null;
        }

        public async Task<Item?> RequestItemSelection(Character character, List<Item> availableItems)
        {
            WriteLine($"请为 {character.NickName} 选择一个物品。");
            long? itemId = await _itemSelectionRequester.RequestInput(
                (callback) => UI.Invoke(() => UI.ShowItemSelectionUI(availableItems, callback))
            );
            return itemId.HasValue ? availableItems.FirstOrDefault(i => i.Id == itemId.Value) : null;
        }

        public async Task RequestContinuePrompt(string message)
        {
            WriteLine(message);
            await _continuePromptRequester.RequestInput(
                (callback) => UI.Invoke(() => UI.ShowContinuePrompt(message, callback))
            );
        }

        // --- GameMapViewer 调用这些方法来解决 UI 输入 ---

        public void ResolveCharacterSelection(long characterId)
        {
            _characterSelectionRequester.ResolveInput(characterId);
            UI.Invoke(() => UI.HideCharacterSelectionPrompt());
        }

        public void ResolveActionType(CharacterActionType actionType)
        {
            _actionTypeRequester.ResolveInput(actionType);
            UI.Invoke(() => UI.HideActionButtons());
        }

        public void ResolveTargetSelection(List<Character> targetIds)
        {
            _targetSelectionRequester.ResolveInput(targetIds);
            UI.Invoke(() => UI.HideTargetSelectionUI());
        }

        public void ResolveSkillSelection(long skillId)
        {
            _skillSelectionRequester.ResolveInput(skillId);
            UI.Invoke(() => UI.HideSkillSelectionUI());
        }

        public void ResolveItemSelection(long itemId)
        {
            _itemSelectionRequester.ResolveInput(itemId);
            UI.Invoke(() => UI.HideItemSelectionUI());
        }

        public void ResolveContinuePrompt()
        {
            _continuePromptRequester.ResolveInput(true); // 任何值都可以，只要完成Task
            UI.Invoke(() => UI.HideContinuePrompt());
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

        public void UpdateBottomInfoPanel()
        {
            UI.Invoke(UI.UpdateBottomInfoPanel);
        }
        
        public void UpdateQueue()
        {
            UI.Invoke(UI.UpdateLeftQueuePanel);
        }
        
        public void UpdateCharacterPositionsOnMap()
        {
            UI.Invoke(UI.UpdateCharacterPositionsOnMap);
        }

        public void SetQueue(Dictionary<Character, double> dict)
        {
            UI.Invoke(() =>
            {
                UI.CharacterQueueData = dict;
            });
        }

        public void SetGameMap(GameMap map)
        {
            UI.Invoke(() =>
            {
                UI.CurrentGameMap = map;
            });
        }

        public void SetPlayerCharacter(Character character)
        {
            UI.Invoke(() =>
            {
                UI.PlayerCharacter = character;
            });
        }
        
        public void SetCurrentCharacter(Character character)
        {
            UI.Invoke(() =>
            {
                UI.CurrentCharacter = character;
            });
        }

        public void SetCharacterStatistics(Dictionary<Character, CharacterStatistics> stats)
        {
            UI.Invoke(() =>
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
