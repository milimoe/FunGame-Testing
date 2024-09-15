using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace FunGame.Testing.Items
{
    public class 攻击之爪10 : Item
    {
        public override long Id => 14001;
        public override string Name => "攻击之爪 +10";
        public override string Description => Skills.Passives.Count > 0 ? Skills.Passives.First().Description : "";

        public 攻击之爪10(Character? character = null) : base(ItemType.Accessory, slot: EquipSlotType.Accessory)
        {
            Skills.Passives.Add(new 攻击之爪技能(character, this, 10));
        }
    }

    public class 攻击之爪30 : Item
    {
        public override string Name => "攻击之爪 +30";
        public override string Description => Skills.Passives.Count > 0 ? Skills.Passives.First().Description : "";

        public 攻击之爪30(Character? character = null) : base(ItemType.Accessory, slot: EquipSlotType.Accessory)
        {
            Skills.Passives.Add(new 攻击之爪技能(character, this, 30));
        }
    }

    public class 攻击之爪50 : Item
    {
        public override string Name => "攻击之爪 +50";
        public override string Description => Skills.Passives.Count > 0 ? Skills.Passives.First().Description : "";

        public 攻击之爪50(Character? character = null) : base(ItemType.Accessory, slot: EquipSlotType.Accessory)
        {
            Skills.Passives.Add(new 攻击之爪技能(character, this, 50));
        }
    }

    public class 攻击之爪技能 : Skill
    {
        public override long Id => 5001;
        public override string Name => "攻击之爪";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public Item Item { get; }

        public 攻击之爪技能(Character? character, Item item, double exATK) : base(SkillType.Passive, character)
        {
            Level = 1;
            Item = item;
            Effects.Add(new 攻击之爪特效(this, character, item, exATK));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 攻击之爪特效 : Effect
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"增加角色 {攻击力加成} 点攻击力。" + (!TargetSelf ? $"来自：[ {Source} ] 的 [ {Item.Name} ]" : "");
        public override EffectType EffectType => EffectType.Item;
        public override bool TargetSelf => true;

        public Item Item { get; }
        private readonly double 攻击力加成 = 0;

        public override void OnEffectGained(Character character)
        {
            character.ExATK2 += 攻击力加成;
        }
        
        public override void OnEffectLost(Character character)
        {
            character.ExATK2 -= 攻击力加成;
        }

        public 攻击之爪特效(Skill skill, Character? source, Item item, double exATK) : base(skill)
        {
            ActionQueue = skill.ActionQueue;
            Source = source;
            Item = item;
            攻击力加成 = exATK;
        }
    }
}
