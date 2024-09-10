using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 魔法涌流 : Skill
    {
        public override long Id => 3003;
        public override string Name => "魔法涌流";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => 100;
        public override double CD => 65;
        public override double HardnessTime => 10;

        public 魔法涌流(Character character) : base(SkillType.SuperSkill, character)
        {
            Effects.Add(new 魔法涌流特效(this));
        }
    }

    public class 魔法涌流特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => "魔法涌流";
        public override string Description => $"{Duration} 秒内，增加所有伤害的20%伤害减免，并将普通攻击转为魔法伤害，可叠加魔法震荡的效果。";
        public override bool TargetSelf => true;
        public override bool Durative => true;
        public override double Duration => 50;

        private double 减伤比例 => Calculation.Round2Digits(0.2 + 0.02 * (Level -1));
        private double 实际比例 = 0;

        public override void OnEffectGained(Character character)
        {
            实际比例 = 减伤比例;
            character.NormalAttack.SetMagicType(true, character.MagicType);
        }

        public override void OnEffectLost(Character character)
        {
            character.NormalAttack.SetMagicType(false, character.MagicType);
        }

        public override void AlterActualDamageAfterCalculation(Character character, Character enemy, ref double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, bool isCritical)
        {
            if (enemy == Skill.Character)
            {
                damage = Calculation.Round2Digits(damage * (1 - 实际比例));
            }
        }

        public override void OnSkillCasted(Character actor, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
        {
            RemainDuration = Duration;
            if (!actor.Effects.Contains(this))
            {
                实际比例 = 0;
                actor.Effects.Add(this);
                OnEffectGained(actor);
            }
        }
    }
}
