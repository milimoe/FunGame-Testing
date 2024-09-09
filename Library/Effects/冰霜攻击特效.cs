using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects
{
    public class 冰霜攻击特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => "冰霜攻击";
        public override string Description => $"对目标敌人造成 {Calculation.Round2Digits(90 + 60 * (Skill.Level - 1))} + {Calculation.Round2Digits((1.2 + 1.8 * (Skill.Level - 1)) * 100)}%智力 [ {Damage} ] 点{CharacterSet.GetMagicName(MagicType)}。";
        public override bool TargetSelf => false;
        public override int TargetCount => 1;

        private double Damage
        {
            get
            {
                double d = 0;
                if (Skill.Character != null)
                {
                    d = Calculation.Round2Digits(90 + 60 * (Skill.Level - 1) + (1.2 + 1.8 * (Skill.Level - 1)) * Skill.Character.INT);
                }
                return d;
            }
        }

        public override void OnSkillCasted(ActionQueue queue, Character actor, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
        {
            Character enemy = enemys[new Random().Next(enemys.Count)];
            double damageBase = Damage;
            if (queue.CalculateMagicalDamage(actor, enemy, false, MagicType, damageBase, out double damage) != DamageResult.Evaded)
            {
                queue.DamageToEnemy(actor, enemy, damage, false, true, MagicType);
            }
        }
    }
}
