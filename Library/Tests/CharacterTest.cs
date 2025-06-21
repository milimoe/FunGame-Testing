using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Oshima.FunGame.OshimaModules.Characters;
using Oshima.FunGame.OshimaModules.Effects.PassiveEffects;
using Oshima.FunGame.OshimaModules.Skills;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    internal class CharacterTest
    {
        public CharacterTest()
        {
            GamingQueue queue = new(Console.WriteLine);
            Character character = new CustomCharacter(0, "");
            character.SetLevel(60);
            Console.WriteLine(character.GetInfo());
            Item item = FunGameConstant.Equipment.Where(i => i.Id == 12515).First();
            character.Equip(item);
            Console.WriteLine(character.GetInfo());
            Character enemy = new CustomCharacter(1, "敌人");
            Console.ReadKey();
            enemy.SetLevel(60);
            Skill skill = new 虚弱领域(enemy);
            skill.Level += 8;
            skill.OnSkillCasted(queue, enemy, [character]);
            character.UnEquip(EquipSlotType.Armor);
            Console.WriteLine(character.GetInfo());
            Console.ReadKey();
            character.Equip(item);
            Console.WriteLine(character.GetInfo());
            Console.ReadKey();
            Effect e = character.Effects.First(e => e is 虚弱);
            character.Effects.Remove(e);
            e.OnEffectLost(character);
            Console.WriteLine(character.GetInfo());
            Console.ReadKey();
            character.UnEquip(EquipSlotType.Armor);
            Console.WriteLine(character.GetInfo());
            Console.ReadKey();
        }
    }
}
