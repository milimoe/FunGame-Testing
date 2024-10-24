﻿using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;

namespace Milimoe.FunGame.Testing.Effects
{
    public class 攻击力加成 : Effect
    {
        public override long Id => Skill.Id;
        public override string Name => Skill.Name;
        public override string Description => $"增加角色 {实际攻击力加成} 点攻击力。" + (!TargetSelf ? $"来自：[ {Source} ]" + (Item != null ? $" 的 [ {Item.Name} ]" : "") : "");
        public override EffectType EffectType => EffectType.Item;
        public override bool TargetSelf => true;

        public Item? Item { get; }
        private readonly double 实际攻击力加成 = 0;

        public override void OnEffectGained(Character character)
        {
            character.ExATK2 += 实际攻击力加成;
        }

        public override void OnEffectLost(Character character)
        {
            character.ExATK2 -= 实际攻击力加成;
        }

        public 攻击力加成(Skill skill, Character? source, Item? item, double exATK) : base(skill)
        {
            GamingQueue = skill.GamingQueue;
            Source = source;
            Item = item;
            实际攻击力加成 = exATK;
        }
    }
}
