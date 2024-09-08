using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Testing.Effects;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 冰霜攻击 : Skill
    {
        public override long Id => 1;
        public override string Name => "冰霜攻击";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double MPCost => BaseMPCost + (50 * (Level - 1));
        public override double CD => 20;
        public override double CastTime => 6;
        public override double HardnessTime => 3;
        protected override double BaseMPCost => 30;

        public 冰霜攻击(Character character) : base(true, true, character)
        {
            Effects.Add(new 冰霜攻击特效(this));
        }
    }
}
