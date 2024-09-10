using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 毁灭之势 : Skill
    {
        public override long Id => 4007;
        public override string Name => "毁灭之势";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 毁灭之势(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 毁灭之势特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 毁灭之势特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"每时间提升 2.5% 所有伤害，无上限，但受到伤害时效果清零。";
        public override bool TargetSelf => true;
    }
}
