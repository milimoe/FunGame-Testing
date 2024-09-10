using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 心灵之火 : Skill
    {
        public override long Id => 4002;
        public override string Name => "心灵之火";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 心灵之火(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 心灵之火特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 心灵之火特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"普通攻击硬直时间减少 20%。";
        public override bool TargetSelf => true;

        public override void AlterHardnessTimeAfterNormalAttack(Character character, ref double baseHardnessTime)
        {
            baseHardnessTime = Calculation.Round2Digits(baseHardnessTime * 0.8);
        }
    }
}
