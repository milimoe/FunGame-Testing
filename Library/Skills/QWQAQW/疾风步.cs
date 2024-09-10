﻿using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 疾风步 : Skill
    {
        public override long Id => 2001;
        public override string Name => "疾风步";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => 35;
        public override double CD => 35;
        public override double HardnessTime => 5;

        public 疾风步(Character character) : base(SkillType.Skill, character)
        {
            Effects.Add(new 疾风步特效(this));
        }
    }

    public class 疾风步特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"进入不可选中状态，获得 100 行动速度，提高 15% 闪避率和 15% 暴击率，持续 {Duration} 时间。破隐一击：在持续时间内，首次造成伤害会附加 {Calculation.Round2Digits((1.5 + 1.5 * (Skill.Level - 1)) * 100)}% 敏捷 [ {伤害加成} ] 的强化伤害，并解除不可选中状态。";
        public override bool TargetSelf => true;
        public override bool Durative => true;
        public override double Duration => 15 + (2 * (Level - 1));

        private double 伤害加成
        {
            get
            {
                double d = 0;
                if (Skill.Character != null)
                {
                    d = Calculation.Round2Digits((1.5 + 1.5 * (Skill.Level - 1)) * Skill.Character.AGI);
                }
                return d;
            }
        }
        private bool 首次伤害 { get; set; } = true;
        private bool 破隐一击 { get; set; } = false;

        public override void OnEffectGained(Character character)
        {
            Skill.IsInEffect = true;
            character.IsUnselectable = true;
            character.ExSPD += 100;
            character.ExEvadeRate += 0.15;
            character.ExCritRate += 0.15;
        }

        public override void OnEffectLost(Character character)
        {
            Skill.IsInEffect = false;
            if (!破隐一击)
            {
                // 在没有打出破隐一击的情况下，恢复角色状态
                character.IsUnselectable = false;
            }
            character.ExSPD -= 100;
            character.ExEvadeRate -= 0.15;
            character.ExCritRate -= 0.15;
        }

        public override void AlterActualDamageAfterCalculation(Character character, Character enemy, ref double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, DamageResult damageResult)
        {
            if (character == Skill.Character && 首次伤害)
            {
                首次伤害 = false;
                破隐一击 = true;
                character.IsUnselectable = false;
                double d = 伤害加成;
                damage = Calculation.Round2Digits(damage + d);
                WriteLine($"[ {character} ] 触发了疾风步破隐一击，获得了 [ {d} ] 点伤害加成！");
            }
        }

        public override void OnSkillCasted(Character actor, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
        {
            if (!actor.Effects.Contains(this))
            {
                首次伤害 = true;
                破隐一击 = false;
                RemainDuration = Duration;
                actor.Effects.Add(this);
                OnEffectGained(actor);
            }
        }
    }
}
