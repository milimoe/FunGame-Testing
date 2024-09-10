using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 智慧与力量 : Skill
    {
        public override long Id => 4005;
        public override string Name => "智慧与力量";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 智慧与力量(Character character) : base(SkillType.Passive, character)
        {
            Effects.Add(new 智慧与力量特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }

    public class 智慧与力量特效(Skill skill) : Effect(skill)
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"当生命值低于 30% 时，智力转化为力量；当生命值高于或等于 30% 时，力量转化为智力。";
        public override bool TargetSelf => true;
    }
}
