using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;
using Milimoe.FunGame.Core.Library.Constant;
using Milimoe.FunGame.Core.Model;
using Oshima.FunGame.OshimaModules.Characters;
using Oshima.FunGame.OshimaModules.Effects.PassiveEffects;
using Oshima.FunGame.OshimaModules.Regions;
using Oshima.FunGame.OshimaModules.Skills;
using Oshima.FunGame.OshimaServers.Model;
using Oshima.FunGame.OshimaServers.Service;

namespace Milimoe.FunGame.Testing.Tests
{
    internal class CharacterTest
    {
        public static async Task CharacterTest1()
        {
            GamingQueue queue = new(Console.WriteLine);
            Character character = new CustomCharacter(0, "");
            character.SetLevel(60);
            Console.WriteLine(character.GetInfo());
            Item item = FunGameConstant.Equipment.Where(i => i.Id == 12515).First();
            character.Equip(item);
            Console.WriteLine(character.GetInfo());
            Character teammate = new MagicalGirl();
            Console.ReadKey();
            teammate.SetLevel(60);
            Skill skill = new 毁灭之势(teammate);
            teammate.Skills.Add(skill);
            skill.GamingQueue = queue;
            skill.Character = teammate;
            skill.Level++;
            skill = new 绝对领域(teammate);
            teammate.Skills.Add(skill);
            skill.GamingQueue = queue;
            skill.Character = teammate;
            skill.Level += 6;
            skill.OnSkillCasted(queue, teammate, [character]);
            skill.OnSkillCasted(queue, teammate, [character]);
            Character enemy = new CustomCharacter(1, "敌人");
            Console.ReadKey();
            enemy.SetLevel(60);
            skill = new 混沌烙印(enemy);
            skill.GamingQueue = queue;
            skill.Level += 8;
            skill.OnSkillCasted(queue, enemy, [teammate]);
            queue.CharacterStatistics[teammate] = new CharacterStatistics();
            queue.AddCharacter(teammate, 10);
            await queue.TimeLapse();
            Console.WriteLine(teammate.GetInfo());
            skill = new 虚弱领域(enemy);
            skill.GamingQueue = queue;
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

        public static async Task CharacterTest2()
        {
            Character character1 = new CustomCharacter(1, "测试1")
            {
                Id = Random.Shared.Next(1, 13),
                Level = 60,
            };
            Character character2 = new CustomCharacter(1, "测试A")
            {
                Id = Random.Shared.Next(1, 13),
                Level = 60
            };
            Character character3 = new CustomCharacter(1, "测试α")
            {
                Id = Random.Shared.Next(1, 13),
                Level = 60
            };
            Character character4 = new CustomCharacter(1, "测试Ⅰ")
            {
                Id = Random.Shared.Next(1, 13),
                Level = 60
            };
            character1.NormalAttack.Level = 8;
            character2.NormalAttack.Level = 8;
            character3.NormalAttack.Level = 8;
            character4.NormalAttack.Level = 8;
            character1.Recovery();
            character2.Recovery();
            character3.Recovery();
            character4.Recovery();
            GamingQueue queue = new([character1, character2, character3, character4], Console.WriteLine);
            FunGameService.AddCharacterSkills(character1, 1, 6, 6);
            FunGameService.AddCharacterSkills(character2, 1, 6, 6);
            FunGameService.AddCharacterSkills(character3, 1, 6, 6);
            FunGameService.AddCharacterSkills(character4, 1, 6, 6);
            FunGameSimulation.DropItems(queue, 5, 5, 5, 5, 5);
            FunGameService.SetCharacterPrimaryAttribute(character1);
            FunGameService.SetCharacterPrimaryAttribute(character2);
            FunGameService.SetCharacterPrimaryAttribute(character3);
            FunGameService.SetCharacterPrimaryAttribute(character4);
            Console.WriteLine(character1.GetInfo());
            Console.WriteLine(character2.GetInfo());
            Console.WriteLine(character3.GetInfo());
            Console.WriteLine(character4.GetInfo());
            User user = Factory.GetUser(1, "测试用户");
            if (character1.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character1.EquipSlot.MagicCardPack);
            if (character1.EquipSlot.Weapon != null) user.Inventory.Items.Add(character1.EquipSlot.Weapon);
            if (character1.EquipSlot.Armor != null) user.Inventory.Items.Add(character1.EquipSlot.Armor);
            if (character1.EquipSlot.Shoes != null) user.Inventory.Items.Add(character1.EquipSlot.Shoes);
            if (character1.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character1.EquipSlot.Accessory1);
            if (character1.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character1.EquipSlot.Accessory2);
            if (character2.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character2.EquipSlot.MagicCardPack);
            if (character2.EquipSlot.Weapon != null) user.Inventory.Items.Add(character2.EquipSlot.Weapon);
            if (character2.EquipSlot.Armor != null) user.Inventory.Items.Add(character2.EquipSlot.Armor);
            if (character2.EquipSlot.Shoes != null) user.Inventory.Items.Add(character2.EquipSlot.Shoes);
            if (character2.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character2.EquipSlot.Accessory1);
            if (character2.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character2.EquipSlot.Accessory2);
            if (character3.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character3.EquipSlot.MagicCardPack);
            if (character3.EquipSlot.Weapon != null) user.Inventory.Items.Add(character3.EquipSlot.Weapon);
            if (character3.EquipSlot.Armor != null) user.Inventory.Items.Add(character3.EquipSlot.Armor);
            if (character3.EquipSlot.Shoes != null) user.Inventory.Items.Add(character3.EquipSlot.Shoes);
            if (character3.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character3.EquipSlot.Accessory1);
            if (character3.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character3.EquipSlot.Accessory2);
            if (character4.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character4.EquipSlot.MagicCardPack);
            if (character4.EquipSlot.Weapon != null) user.Inventory.Items.Add(character4.EquipSlot.Weapon);
            if (character4.EquipSlot.Armor != null) user.Inventory.Items.Add(character4.EquipSlot.Armor);
            if (character4.EquipSlot.Shoes != null) user.Inventory.Items.Add(character4.EquipSlot.Shoes);
            if (character4.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character4.EquipSlot.Accessory1);
            if (character4.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character4.EquipSlot.Accessory2);
            user.Inventory.Characters.Add(character1);
            user.Inventory.Characters.Add(character2);
            user.Inventory.Characters.Add(character3);
            user.Inventory.Characters.Add(character4);
            while (true)
            {
                character1.Recovery();
                character2.Recovery();
                character3.Recovery();
                character4.Recovery();
                OshimaRegion region = FunGameConstant.Regions.OrderBy(o => Random.Shared.Next()).First();
                ExploreModel model = await FunGameService.GenerateExploreModel(region, [1, 2, 3, 4], user);
                Console.WriteLine(model.GetExploreInfo(user.Inventory.Characters, FunGameConstant.Regions));
                Console.WriteLine(model.String);
                ConsoleKey key = Console.ReadKey().Key;
                if (key == ConsoleKey.Escape)
                {
                    break;
                }
                else if (key == ConsoleKey.F3)
                {
                    character1.Items.Clear();
                    character2.Items.Clear();
                    character3.Items.Clear();
                    character4.Items.Clear();
                    FunGameSimulation.DropItems(queue, 5, 5, 5, 5, 5);
                    FunGameService.SetCharacterPrimaryAttribute(character1);
                    FunGameService.SetCharacterPrimaryAttribute(character2);
                    FunGameService.SetCharacterPrimaryAttribute(character3);
                    FunGameService.SetCharacterPrimaryAttribute(character4);
                    Console.WriteLine(character1.GetInfo());
                    Console.WriteLine(character2.GetInfo());
                    Console.WriteLine(character3.GetInfo());
                    Console.WriteLine(character4.GetInfo());
                    if (character1.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character1.EquipSlot.MagicCardPack);
                    if (character1.EquipSlot.Weapon != null) user.Inventory.Items.Add(character1.EquipSlot.Weapon);
                    if (character1.EquipSlot.Armor != null) user.Inventory.Items.Add(character1.EquipSlot.Armor);
                    if (character1.EquipSlot.Shoes != null) user.Inventory.Items.Add(character1.EquipSlot.Shoes);
                    if (character1.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character1.EquipSlot.Accessory1);
                    if (character1.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character1.EquipSlot.Accessory2);
                    if (character2.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character2.EquipSlot.MagicCardPack);
                    if (character2.EquipSlot.Weapon != null) user.Inventory.Items.Add(character2.EquipSlot.Weapon);
                    if (character2.EquipSlot.Armor != null) user.Inventory.Items.Add(character2.EquipSlot.Armor);
                    if (character2.EquipSlot.Shoes != null) user.Inventory.Items.Add(character2.EquipSlot.Shoes);
                    if (character2.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character2.EquipSlot.Accessory1);
                    if (character2.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character2.EquipSlot.Accessory2);
                    if (character3.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character3.EquipSlot.MagicCardPack);
                    if (character3.EquipSlot.Weapon != null) user.Inventory.Items.Add(character3.EquipSlot.Weapon);
                    if (character3.EquipSlot.Armor != null) user.Inventory.Items.Add(character3.EquipSlot.Armor);
                    if (character3.EquipSlot.Shoes != null) user.Inventory.Items.Add(character3.EquipSlot.Shoes);
                    if (character3.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character3.EquipSlot.Accessory1);
                    if (character3.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character3.EquipSlot.Accessory2);
                    if (character4.EquipSlot.MagicCardPack != null) user.Inventory.Items.Add(character4.EquipSlot.MagicCardPack);
                    if (character4.EquipSlot.Weapon != null) user.Inventory.Items.Add(character4.EquipSlot.Weapon);
                    if (character4.EquipSlot.Armor != null) user.Inventory.Items.Add(character4.EquipSlot.Armor);
                    if (character4.EquipSlot.Shoes != null) user.Inventory.Items.Add(character4.EquipSlot.Shoes);
                    if (character4.EquipSlot.Accessory1 != null) user.Inventory.Items.Add(character4.EquipSlot.Accessory1);
                    if (character4.EquipSlot.Accessory2 != null) user.Inventory.Items.Add(character4.EquipSlot.Accessory2);
                }
            }
        }
    }
}
