using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Testing.Effects;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 疾风步 : Skill
    {
        public override long Id => 4001;
        public override string Name => "疾风步";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";
        public override double EPCost => 35;
        public override double CD => 35;
        public override double HardnessTime => 5;

        public 疾风步(Character character) : base(true, false, character)
        {
            Effects.Add(new 疾风步特效(this));
        }
    }
}
