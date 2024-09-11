using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 敏捷之刃 : Skill
    {
        public override long Id => 4011;
        public override string Name => "敏捷之刃";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 敏捷之刃(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 敏捷之刃特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 敏捷之刃特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"每次普通攻击都将附带基于敏捷的魔法伤害。";
        public override bool TargetSelf => true;
    }
}
