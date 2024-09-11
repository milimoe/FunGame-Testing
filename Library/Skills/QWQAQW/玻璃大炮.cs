using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 玻璃大炮 : Skill
    {
        public override long Id => 4009;
        public override string Name => "玻璃大炮";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 玻璃大炮(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 玻璃大炮特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 玻璃大炮特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"生命值高于30%时，受到额外的 [ 20~40% ] 伤害，但是获得 [ 上次所受伤害的 80%  ] 伤害加成；生命值低于等于30%时，不会受到额外的伤害，但是仅能获得 [ 上次所受伤害的 30% ] 伤害加成。" +
            $"在没有受到任何伤害的时候，将获得 {常规伤害加成 * 100:f2}% 伤害加成。（当前伤害加成：{伤害加成 * 100:f2}%）";
        public override bool TargetSelf => true;

        private double 上次受到的伤害 = 0;
        private double 这次的伤害加成 = 0;
        private double 这次受到的额外伤害 = 0;
        private readonly double 常规伤害加成 = 0.2;
        private readonly double 高于30的加成 = 0.8;
        private readonly double 低于30的加成 = 0.3;

        private double 伤害加成
        {
            get
            {
                double 系数 = 常规伤害加成;
                Character? character = Skill.Character;
                if (character is null) return 系数;
                if (上次受到的伤害 != 0)
                {
                    if (character.HP > character.MaxHP * 0.3)
                    {
                        系数 = 高于30的加成;
                    }
                    else
                    {
                        系数 = 低于30的加成;
                    }
                }
                return 系数;
            }
        }

        public override void AlterExpectedDamageBeforeCalculation(Character character, Character enemy, ref double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType)
        {
            if (character == Skill.Character)
            {
                这次的伤害加成 = Calculation.Round2Digits(damage * 伤害加成);
                damage = Calculation.Round2Digits(damage + 这次的伤害加成);
                WriteLine($"[ {character} ] 发动了玻璃大炮，获得了 {这次的伤害加成} 点伤害加成！");
            }

            if (enemy == Skill.Character)
            {
                if (character.HP > character.MaxHP * 0.3)
                {
                    // 额外受到伤害
                    double 系数 = Calculation.Round4Digits((new Random().Next(20, 40) + 0.0) / 100);
                    这次受到的额外伤害 = Calculation.Round2Digits(damage * 系数);
                    damage = Calculation.Round2Digits(damage + 这次受到的额外伤害);
                    WriteLine($"[ {character} ] 的玻璃大炮触发，将额外受到 {这次受到的额外伤害} 点伤害！");
                }
            }
        }

        public override void AfterDamageCalculation(Character character, Character enemy, double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, DamageResult damageResult)
        {
            if (enemy == Skill.Character && damageResult != DamageResult.Evaded)
            {
                上次受到的伤害 = damage;
            }
        }
    }
}
