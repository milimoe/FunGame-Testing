using Milimoe.FunGame.Testing.Effects;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Items
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
            Effects.Add(new 攻击力加成(this, character, item, exATK));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }
}
