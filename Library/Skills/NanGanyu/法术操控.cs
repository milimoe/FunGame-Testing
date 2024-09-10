using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 法术操控 : Skill
    {
        public override long Id => 4004;
        public override string Name => "法术操控";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 法术操控(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 法术操控特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 法术操控特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"每释放两次魔法才会触发硬直时间，且魔法命中时基于智力获得额外能量值。";
        public override bool TargetSelf => true;
    }
}
