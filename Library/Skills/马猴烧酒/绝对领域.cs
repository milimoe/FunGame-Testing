﻿using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 绝对领域 : Skill
    {
        public override long Id => (long)SuperSkillID.绝对领域;
        public override string Name => "绝对领域";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => Math.Max(100, Character?.EP ?? 100);
        public override double CD => 32 + (1 * (Level - 1));
        public override double HardnessTime { get; set; } = 12;

        public 绝对领域(Character? character = null) : base(SkillType.SuperSkill, character)
        {
            Effects.Add(new 绝对领域特效(this));
        }
    }

    public class 绝对领域特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"{Duration} 时间内无法受到任何伤害，且敏捷提升 {系数 * 100:0.##}% [ {敏捷提升} ]。此技能会消耗至少 100 点能量。";
        public override bool TargetSelf => true;
        public override bool Durative => true;
        public override double Duration => Calculation.Round2Digits(16 + 释放时的能量值 * 0.03);

        private double 系数 => Calculation.Round4Digits(0.3 + 0.03 * (Level - 1));
        private double 敏捷提升 => Calculation.Round2Digits(系数 * Skill.Character?.BaseAGI ?? 0);
        private double 实际敏捷提升 = 0;
        private double 释放时的能量值 = 0;

        public override void OnEffectGained(Character character)
        {
            实际敏捷提升 = 敏捷提升;
            character.ExAGI += 实际敏捷提升;
            WriteLine($"[ {character} ] 的敏捷提升了 {系数 * 100:0.##}% [ {实际敏捷提升} ] ！");
        }

        public override void OnEffectLost(Character character)
        {
            character.ExAGI -= 实际敏捷提升;
        }

        public override bool AlterActualDamageAfterCalculation(Character character, Character enemy, ref double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, DamageResult damageResult)
        {
            if (enemy == Skill.Character && damageResult != DamageResult.Evaded)
            {
                WriteLine($"[ {enemy} ] 发动了绝对领域，巧妙的化解了此伤害！");
                return true;
            }
            return false;
        }

        public override void OnSkillCasting(Character caster)
        {
            释放时的能量值 = caster.EP;
        }

        public override void OnSkillCasted(Character caster, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
        {
            RemainDuration = Duration;
            if (!caster.Effects.Contains(this))
            {
                实际敏捷提升 = 0;
                caster.Effects.Add(this);
                OnEffectGained(caster);
            }
        }
    }
}
