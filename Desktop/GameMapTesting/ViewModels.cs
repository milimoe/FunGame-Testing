using System.ComponentModel;
using System.Runtime.CompilerServices;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Interface.Entity;

namespace Milimoe.FunGame.Testing.Desktop.GameMapTesting
{
    public class CharacterSkillsAndItemsViewModel : INotifyPropertyChanged
    {
        public List<ISkill> Skills
        {
            get => _skills;
            set
            {
                if (_skills != value)
                {
                    _skills = value;
                    OnPropertyChanged();
                }
            }
        }
        private List<ISkill> _skills = [];
        
        public List<Item> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }
        private List<Item> _items = [];

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CharacterQueueItemViewModel(CharacterQueueItem model, Func<Dictionary<int, List<Skill>>> getTurnRewards) : INotifyPropertyChanged
    {
        public CharacterQueueItem Model { get; } = model ?? throw new ArgumentNullException(nameof(model));
        public Character Character => Model.Character;
        public double ATDelay => Model.ATDelay;

        private int _predictedTurnNumber;
        public int PredictedTurnNumber
        {
            get => _predictedTurnNumber;
            set
            {
                if (_predictedTurnNumber != value)
                {
                    _predictedTurnNumber = value;
                    OnPropertyChanged();
                    // 当回合数变化时，奖励信息可能也变化，因此需要更新
                    UpdateRewardProperties();
                }
            }
        }

        private string _turnRewardSkillName = "";
        public string TurnRewardSkillName
        {
            get => _turnRewardSkillName;
            set
            {
                if (_turnRewardSkillName != value)
                {
                    _turnRewardSkillName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasTurnReward)); // 奖励名称变化时，通知可见性也可能变化
                }
            }
        }

        public bool HasTurnReward => !string.IsNullOrEmpty(TurnRewardSkillName);

        // 用于获取 TurnRewards 字典的委托，避免直接依赖 GameMapViewer
        private readonly Func<Dictionary<int, List<Skill>>> _getTurnRewards = getTurnRewards ?? throw new ArgumentNullException(nameof(getTurnRewards));

        // 当 PredictedTurnNumber 或 TurnRewards 变化时调用此方法
        public void UpdateRewardProperties()
        {
            Dictionary<int, List<Skill>> turnRewards = _getTurnRewards();
            if (turnRewards != null && turnRewards.TryGetValue(PredictedTurnNumber, out List<Skill>? rewardSkills))
            {
                TurnRewardSkillName = string.Join("；", rewardSkills.Select(s => s.Name.Replace("[R]", "").Trim()));
            }
            else
            {
                TurnRewardSkillName = "";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CharacterQueueItem(Character character, double atDelay)
    {
        public Character Character { get; set; } = character;
        public double ATDelay { get; set; } = atDelay;
    }
}
