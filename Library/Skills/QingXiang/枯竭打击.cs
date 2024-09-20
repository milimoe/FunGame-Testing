﻿using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 枯竭打击 : Skill
    {
        public override long Id => (long)PassiveID.枯竭打击;
        public override string Name => "枯竭打击";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 枯竭打击(Character? character = null) : base(SkillType.Passive, character)
        {
            Effects.Add(new 枯竭打击特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 枯竭打击特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"每次造成伤害都会随机减少对方 [ 10~25 ] 点能量值，对能量值低于一半的角色额外造成 30% 伤害。对于枯竭打击而言，能量值大于100且小于150时，视为低于一半。";
        public override bool TargetSelf => true;

        private bool 是否是嵌套伤害 = false;

        public override void AfterDamageCalculation(Character character, Character enemy, double damage, bool isNormalAttack, bool isMagicDamage, MagicType magicType, DamageResult damageResult)
        {
            if (character == Skill.Character && damageResult != DamageResult.Evaded && !是否是嵌套伤害)
            {
                // 减少能量
                double EP = new Random().Next(10, 25);
                enemy.EP -= EP;
                WriteLine($"[ {character} ] 发动了枯竭打击！[ {enemy} ] 的能量值被减少了 {EP} 点！现有能量：{enemy.EP}。");
                // 额外伤害
                if (enemy.EP >= 0 && enemy.EP < 50 || enemy.EP >= 100 && enemy.EP < 150)
                {
                    double 额外伤害 = Calculation.Round2Digits(damage * 0.3);
                    WriteLine($"[ {character} ] 发动了枯竭打击！将造成额外伤害！");
                    是否是嵌套伤害 = true;
                    DamageToEnemy(character, enemy, isMagicDamage, magicType, 额外伤害);
                }
            }

            if (character == Skill.Character && 是否是嵌套伤害)
            {
                是否是嵌套伤害 = false;
            }
        }
    }
}
