using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects
{
    public class 天赐之力特效(Skill skill) : Effect(skill)
    {
        public override long Id => 1;
        public override string Name => "天赐之力";
        public override string Description => $"{Duration} 时间内，获得 25% 闪避率（不可叠加），普通攻击硬直时间额外减少 20%，基于 {Calculation.Round4Digits((1.2 + (1 + 0.6 * (Skill.Level - 1))) * 100)}% 核心属性 [ {伤害加成} ] 强化普通攻击的伤害。";
        public override bool TargetSelf => false;
        public override int TargetCount => 1;
        public override bool Durative => true;
        public override double Duration => 40;
        public override MagicType MagicType => MagicType.Element;

        private double 伤害加成
        {
            get
            {
                double d = 0;
                if (Skill.Character != null)
                {
                    d = Calculation.Round2Digits(1.2 * (1 + 0.6 * (Skill.Level - 1)) * Skill.Character.PrimaryAttributeValue);
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

        public override bool AlterExpectedDamageBeforeCalculation(Character character, Character enemy, double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, out double newDamage)
        {
            newDamage = damage;
            if (isNormalAttack)
            {
                newDamage = Calculation.Round2Digits(damage + 伤害加成);
            }
            return true;
        }

        public override bool AlterHardnessTimeAfterNormalAttack(Character character, double baseHardnessTime, out double newHardnessTime)
        {
            newHardnessTime = Calculation.Round2Digits(baseHardnessTime * 0.8);
            return true;
        }

        public override void OnSkillCasted(ActionQueue queue, Character actor, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
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
