﻿using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Testing.Effects.ItemEffects;

namespace FunGame.Testing.Items
{
    public class 独奏弓 : Item
    {
        public override long Id => 11001;
        public override string Name => "独奏弓";
        public override string Description => Skills.Passives.Count > 0 ? Skills.Passives.First().Description : "";

        public 独奏弓(Character? character = null) : base(ItemType.Weapon)
        {
            WeaponType = WeaponType.Bow;
            Skills.Passives.Add(new 独奏弓技能(character, this));
        }
    }

    public class 独奏弓技能 : Skill
    {
        public override long Id => 5002;
        public override string Name => "独奏弓";
        public override string Description => $"增加角色 {攻击力加成} 点攻击力，减少普通攻击 {硬直时间减少} 硬直时间。";

        private readonly double 攻击力加成 = 80;
        private readonly double 硬直时间减少 = 2;

        public 独奏弓技能(Character? character, Item item) : base(SkillType.Passive, character)
        {
            Level = 1;
            Item = item;
            Effects.Add(new 攻击力加成(this, 攻击力加成, character, item));
            Effects.Add(new 普攻硬直时间减少(this, 硬直时间减少, character, item));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }
}
