using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Effects
{
    public class 大岛特性特效(Skill skill) : Effect(skill)
    {
        public override long Id => 1;
        public override string Name => "大岛特性";
        public override string Description => $"META马专属被动：力量+5，力量成长+0.5；在受到伤害时，获得的能量提升50%，每回合开始还能获得额外的 [ {EP} ] 能量值。";
        public override bool TargetSelf => true;
        public static double EP => 10;

        public override bool AlterEPAfterGetDamage(Character character, double baseEP, out double newEP)
        {
            newEP = Calculation.Round2Digits(baseEP * 1.5);
            if (Skill.Character != null) Console.WriteLine("[ " + Skill.Character + " ] 发动了META马专属被动！本次获得了 " + newEP + " 能量！");
            return true;
        }

        public override void OnTurnStart(Character character)
        {
            if (character.EP < 200)
            {
                character.EP += EP;
                Console.WriteLine("[ " + character + " ] 发动了META马专属被动！本次获得了 " + EP + " 能量！");
            }
        }
    }
}
