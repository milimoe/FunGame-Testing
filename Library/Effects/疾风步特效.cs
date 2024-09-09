using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects
{
    public class 疾风步特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => "疾风步";
        public override string Description => $"进入不可选中状态，获得 100 行动速度，持续 {Duration} 时间。在持续时间内，首次造成伤害会附加 {Calculation.Round2Digits((1.5 + 1.5 * (Skill.Level - 1)) * 100)}% 敏捷 [ {伤害加成} ] 的强化伤害，并解除不可选中状态。剩余的持续时间内，提高 15% 闪避率和暴击率。";
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
            character.IsUnselectable = true;
            Skill.IsInEffect = true;
            character.ExSPD += 100;
        }
        
        public override void OnEffectLost(Character character)
        {
            Skill.IsInEffect = false;
            if (破隐一击)
            {
                character.ExEvadeRate -= 0.15;
                character.ExCritRate -= 0.15;
            }
            else
            {
                character.IsUnselectable = false;
            }
            character.ExSPD -= 100;
        }

        public override bool AlterActualDamageAfterCalculation(Character character, Character enemy, double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, bool isCritical, out double newDamage)
        {
            if (首次伤害)
            {
                首次伤害 = false;
                newDamage = Calculation.Round2Digits(damage + 伤害加成);
                Console.WriteLine($"[ {character} ] 发动了 [ 疾风步 ] 的特效，获得了 [ {伤害加成} ] 点伤害加成！");
                破隐一击 = true;
                character.ExEvadeRate += 0.15;
                character.ExCritRate += 0.15;
                character.IsUnselectable = false;
                return true;
            }
            else
            {
                newDamage = damage;
                return true;
            }
        }

        public override void OnSkillCasted(ActionQueue queue, Character actor, List<Character> enemys, List<Character> teammates, Dictionary<string, object> others)
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
