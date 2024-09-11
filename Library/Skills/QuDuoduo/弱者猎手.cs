using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 弱者猎手 : Skill
    {
        public override long Id => 4012;
        public override string Name => "弱者猎手";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 弱者猎手(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 弱者猎手特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 弱者猎手特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"优先攻击血量更低的角色，对生命值低于自己的角色造成150%伤害。";
        public override bool TargetSelf => true;
    }
}
