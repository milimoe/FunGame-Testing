﻿using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects.OpenEffects
{
    public class ExAGI : Effect
    {
        public override long Id => (long)EffectID.ExAGI;
        public override string Name => "敏捷加成";
        public override string Description => $"增加角色 {实际加成:0.##} 点敏捷。" + (!TargetSelf ? $"来自：[ {Source} ]" + (Item != null ? $" 的 [ {Item.Name} ]" : "") : "");
        public override EffectType EffectType => EffectType.Item;
        public override bool TargetSelf => true;

        public Item? Item { get; }
        private readonly double 实际加成 = 0;

        public override void OnEffectGained(Character character)
        {
            character.ExAGI += 实际加成;
        }

        public override void OnEffectLost(Character character)
        {
            character.ExAGI -= 实际加成;
        }

        public ExAGI(Skill skill, Dictionary<string, object> args, Character? source = null, Item? item = null) : base(skill, args)
        {
            GamingQueue = skill.GamingQueue;
            Source = source;
            Item = item;
            if (Values.Count > 0)
            {
                string key = Values.Keys.FirstOrDefault(s => s.Equals("exagi", StringComparison.CurrentCultureIgnoreCase)) ?? "";
                if (key.Length > 0 && double.TryParse(Values[key].ToString(), out double exAGI))
                {
                    实际加成 = exAGI;
                }
            }
        }
    }
}
