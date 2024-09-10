using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 天赐之力 : Skill
    {
        public override long Id => 3001;
        public override string Name => "天赐之力";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => 100;
        public override double CD => 60;
        public override double HardnessTime => 15;

        public 天赐之力(Character character) : base(SkillType.SuperSkill, character)
        {
            Effects.Add(new 天赐之力特效(this));
        }
    }

    public class 天赐之力特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"{Duration} 时间内，获得 25% 闪避率（不可叠加），普通攻击硬直时间额外减少 20%，基于 {Calculation.Round2Digits((1.2 + (1 + 0.6 * (Skill.Level - 1))) * 100)}% 敏捷 [ {伤害加成} ] 强化普通攻击的伤害。";
        public override bool TargetSelf => false;
        public override int TargetCount => 1;
        public override bool Durative => true;
        public override double Duration => 40;

        private double 伤害加成
        {
            get
            {
                double d = 0;
                if (Skill.Character != null)
                {
                    d = Calculation.Round2Digits(1.2 * (1 + 0.6 * (Skill.Level - 1)) * Skill.Character.AGI);
                }
                return d;
            }
        }

        public override void OnEffectGained(Character character)
        {
            character.ExEvadeRate += 0.25;
        }

        public override void OnEffectLost(Character character)
        {
            character.ExEvadeRate -= 0.25;
        }

        public override void AlterExpectedDamageBeforeCalculation(Character character, Character enemy, ref double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType)
        {
            if (character == Skill.Character && isNormalAttack)
            {
                damage = Calculation.Round2Digits(damage + 伤害加成);
            }
        }

        public override void AlterHardnessTimeAfterNormalAttack(Character character, ref double baseHardnessTime)
        {
            baseHardnessTime = Calculation.Round2Digits(baseHardnessTime * 0.8);
        }

        public override void OnSkillCasted(Character actor, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
        {
            RemainDuration = Duration;
            if (!actor.Effects.Contains(this))
            {
                actor.Effects.Add(this);
                OnEffectGained(actor);
            }
        }
    }
}
