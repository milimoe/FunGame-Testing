﻿using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects.OpenEffects
{
    public class ExMR : Effect
    {
        public override long Id => (long)EffectID.ExMR;
        public override string Name => "魔法回复加成";
        public override string Description => $"增加角色 {实际加成:0.##} 点魔法回复。" + (!TargetSelf ? $"来自：[ {Source} ]" + (Item != null ? $" 的 [ {Item.Name} ]" : "") : "");
        public override EffectType EffectType => EffectType.Item;
        public override bool TargetSelf => true;

        public Item? Item { get; }
        private readonly double 实际加成 = 0;

        public override void OnEffectGained(Character character)
        {
            character.ExMR += 实际加成;
        }

        public override void OnEffectLost(Character character)
        {
            character.ExMR -= 实际加成;
        }

        public ExMR(Skill skill, Dictionary<string, object> args, Character? source = null, Item? item = null) : base(skill, args)
        {
            GamingQueue = skill.GamingQueue;
            Source = source;
            Item = item;
            if (Values.Count > 0)
            {
                string key = Values.Keys.FirstOrDefault(s => s.Equals("exmr", StringComparison.CurrentCultureIgnoreCase)) ?? "";
                if (key.Length > 0 && double.TryParse(Values[key].ToString(), out double exMR))
                {
                    实际加成 = exMR;
                }
            }
        }
    }
}
