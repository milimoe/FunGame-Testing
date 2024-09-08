using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Testing.Effects;

namespace Milimoe.FunGame.Testing.Skills
{
    public class 大岛特性 : Skill
    {
        public override long Id => 1;
        public override string Name => "大岛特性";
        public override string Description => Effects.Count > 0 ? Effects.First().Description : "";

        public 大岛特性(Character character) : base(false, false, character)
        {
            Effects.Add(new 大岛特性特效(this));
        }

        public override IEnumerable<Effect> AddInactiveEffectToCharacter()
        {
            return Effects;
        }
    }
}
