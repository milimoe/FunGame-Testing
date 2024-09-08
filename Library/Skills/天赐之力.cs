using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Testing.Effects;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 天赐之力 : Skill
    {
        public override long Id => 1;
        public override string Name => "天赐之力";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => 100;
        public override double CD => 60;
        public override double HardnessTime => 15;

        public 天赐之力(Character character) : base(true, character)
        {
            Effects.Add(new 天赐之力特效(this));
        }
    }
}
