﻿using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 天赐之力 : Skill
    {
        public override long Id => (long)SuperSkillID.天赐之力;
        public override string Name => "天赐之力";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => 100;
        public override double CD => 60;
        public override double HardnessTime { get; set; } = 15;

        public 天赐之力(Character? character = null) : base(SkillType.SuperSkill, character)
        {
            Effects.Add(new 天赐之力特效(this));
        }
    }

    public class 天赐之力特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"{Duration} 时间内，增加 40% 攻击力 [ {攻击力提升} ]、30% 物理穿透和 25% 闪避率（不可叠加），普通攻击硬直时间额外减少 20%，基于 {系数 * 100:0.##}% 敏捷 [ {伤害加成} ] 强化普通攻击的伤害。在持续时间内，【心灵之火】的冷却时间降低至 3 时间。";
        public override bool TargetSelf => false;
        public override int TargetCount => 1;
        public override bool Durative => true;
        public override double Duration => 40;

        private double 系数 => Calculation.Round4Digits(1.2 * (1 + 0.6 * (Skill.Level - 1)));
        private double 伤害加成 => Calculation.Round2Digits(系数 * Skill.Character?.AGI ?? 0);
        private double 攻击力提升 => Calculation.Round2Digits(0.4 * Skill.Character?.BaseATK ?? 0);
        private double 实际的攻击力提升 = 0;

        public override void OnEffectGained(Character character)
        {
            实际的攻击力提升 = 攻击力提升;
            character.ExATK2 += 实际的攻击力提升;
            character.PhysicalPenetration += 0.3;
            character.ExEvadeRate += 0.25;
            if (character.Effects.Where(e => e is 心灵之火特效).FirstOrDefault() is 心灵之火特效 e)
            {
                e.基础冷却时间 = 3;
                if (e.冷却时间 > e.基础冷却时间) e.冷却时间 = e.基础冷却时间;
            }
        }

        public override void OnEffectLost(Character character)
        {
            character.ExATK2 -= 实际的攻击力提升;
            character.PhysicalPenetration -= 0.3;
            character.ExEvadeRate -= 0.25;
            if (character.Effects.Where(e => e is 心灵之火特效).FirstOrDefault() is 心灵之火特效 e)
            {
                e.基础冷却时间 = 8;
            }
        }

        public override CharacterActionType AlterActionTypeBeforeAction(Character character, CharacterState state, ref bool canUseItem, ref bool canCastSkill, ref double pUseItem, ref double pCastSkill, ref double pNormalAttack)
        {
            pNormalAttack += 0.1;
            return CharacterActionType.None;
        }

        public override void AlterExpectedDamageBeforeCalculation(Character character, Character enemy, ref double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType)
        {
            if (character == Skill.Character && isNormalAttack)
            {
                damage = Calculation.Round2Digits(damage + 伤害加成);
            }
        }

        public override void AlterHardnessTimeAfterNormalAttack(Character character, ref double baseHardnessTime, ref bool isCheckProtected)
        {
            baseHardnessTime = Calculation.Round2Digits(baseHardnessTime * 0.8);
        }

        public override void OnSkillCasted(Character caster, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
        {
            RemainDuration = Duration;
            if (!caster.Effects.Contains(this))
            {
                caster.Effects.Add(this);
                OnEffectGained(caster);
            }
        }
    }
}
