using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects
{
    public class ExSTR : Effect
    {
        public override long Id => 8003;
        public override string Name => "力量加成";
        public override string Description => $"增加角色 {实际力量加成} 点力量。" + (!TargetSelf ? $"来自：[ {Source} ]" + (Item != null ? $" 的 [ {Item.Name} ]" : "") : "");
        public override EffectType EffectType => EffectType.Item;
        public override bool TargetSelf => true;

        public Item? Item { get; }
        private readonly double 实际力量加成 = 0;

        public override void OnEffectGained(Character character)
        {
            character.ExDEF2 += 实际力量加成;
        }

        public override void OnEffectLost(Character character)
        {
            character.ExDEF2 -= 实际力量加成;
        }

        public ExSTR(Skill skill, Character? source, Item? item) : base(skill)
        {
            GamingQueue = skill.GamingQueue;
            Source = source;
            Item = item;
            if (skill.OtherArgs.Count > 0)
            {
                IEnumerable<string> keys = skill.OtherArgs.Keys.Where(s => s.Equals("exstr", StringComparison.CurrentCultureIgnoreCase));
                if (keys.Any() && double.TryParse(skill.OtherArgs[keys.First()].ToString(), out double exSTR))
                {
                    实际力量加成 = exSTR;
                }
            }
        }
    }
}
