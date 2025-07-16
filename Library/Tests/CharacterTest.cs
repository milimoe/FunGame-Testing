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
            Character character1 = new CustomCharacter(1, "测试1-Carry", primaryAttribute: PrimaryAttribute.AGI)
            {
                Id = 2,
                Level = 60,
                FirstRoleType = RoleType.Core,
                InitialHP = 65,
            };
            Character character2 = new CustomCharacter(1, "测试A-Tank", primaryAttribute: PrimaryAttribute.STR)
            {
                Id = 3,
                Level = 60,
                FirstRoleType = RoleType.Guardian,
                InitialHP = 85,
            };
            Character character3 = new CustomCharacter(1, "测试α-Support", primaryAttribute: PrimaryAttribute.INT)
            {
                Id = 10,
                Level = 60,
                FirstRoleType = RoleType.Support,
                SecondRoleType = RoleType.Vanguard,
                InitialHP = 65,
            };
            Character character4 = new CustomCharacter(1, "测试Ⅰ-Medic", primaryAttribute: PrimaryAttribute.INT)
            {
                Id = 4,
                Level = 60,
                FirstRoleType = RoleType.Medic,
                InitialHP = 65,
            };
            character1.NormalAttack.Level = 8;
            character2.NormalAttack.Level = 8;
            character3.NormalAttack.Level = 8;
            character4.NormalAttack.Level = 8;
            character1.Recovery();
            character2.Recovery();
            character3.Recovery();
            character4.Recovery();
            FunGameService.AddCharacterSkills(character1, 1, 6, 6);
            FunGameService.AddCharacterSkills(character2, 1, 6, 6);
            FunGameService.AddCharacterSkills(character3, 1, 6, 6);
            FunGameService.AddCharacterSkills(character4, 1, 6, 6);
            DropItemByCharacterRoleType(character1);
            DropItemByCharacterRoleType(character2);
            DropItemByCharacterRoleType(character3);
            DropItemByCharacterRoleType(character4);
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
                ExploreModel model = new()
                {
                    RegionId = region.Id,
                    CharacterIds = [1, 2, 3 ,4],
                    StartTime = DateTime.Now
                };
                await FunGameService.GenerateExploreModel(model, region, [1, 2, 3, 4], user);
                Console.WriteLine(model.GetExploreInfo(user.Inventory.Characters, FunGameConstant.Regions));
                Console.WriteLine(model.String);
                PluginConfig pc2 = new("exploring", user.Id.ToString());
                pc2.LoadConfig();
                FunGameService.SettleExploreAll(pc2, user, true);
                pc2.SaveConfig();
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
                    DropItemByCharacterRoleType(character1);
                    DropItemByCharacterRoleType(character2);
                    DropItemByCharacterRoleType(character3);
                    DropItemByCharacterRoleType(character4);
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

        public static void DropItemByCharacterRoleType(Character character)
        {
            long[] magicIds;
            (int str, int agi, int intelligence)[] values;
            Item? m, w, a, s, ac1, ac2;
            switch (character.FirstRoleType)
            {
                case RoleType.Core:
                    // 核心
                    magicIds = [1031, 1037, 1036];
                    values = [(0, 39, 0), (0, 40, 0), (0, 41, 0)];
                    m = FunGameService.GenerateMagicCardPack(3, QualityType.Red, magicIds, values);
                    if (m != null) character.Equip(m);
                    w = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 11573);
                    if (w != null) character.Equip(w);
                    a = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 12526);
                    if (a != null) character.Equip(a);
                    s = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 13525);
                    if (s != null) character.Equip(s);
                    ac1 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14545);
                    if (ac1 != null) character.Equip(ac1);
                    ac2 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14545);
                    if (ac2 != null) character.Equip(ac2);
                    break;
                case RoleType.Guardian:
                    // 近卫
                    magicIds = [1004, 1038, 1010];
                    values = [(39, 0, 0), (40, 0, 0), (41, 0, 0)];
                    m = FunGameService.GenerateMagicCardPack(3, QualityType.Red, magicIds, values);
                    if (m != null) character.Equip(m);
                    w = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 11579);
                    if (w != null) character.Equip(w);
                    a = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 12525);
                    if (a != null) character.Equip(a);
                    s = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 13526);
                    if (s != null) character.Equip(s);
                    ac1 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14552);
                    if (ac1 != null) character.Equip(ac1);
                    ac2 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14552);
                    if (ac2 != null) character.Equip(ac2);
                    break;
                case RoleType.Support:
                    // 辅助
                    magicIds = [1049, 1020, 1031];
                    values = [(37, 0, 2), (0, 10, 30), (0, 10, 31)];
                    m = FunGameService.GenerateMagicCardPack(3, QualityType.Red, magicIds, values);
                    if (m != null) character.Equip(m);
                    w = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 11578);
                    if (w != null) character.Equip(w);
                    a = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 12526);
                    if (a != null) character.Equip(a);
                    s = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 13523);
                    if (s != null) character.Equip(s);
                    ac1 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14542);
                    if (ac1 != null) character.Equip(ac1);
                    ac2 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14542);
                    if (ac2 != null) character.Equip(ac2);
                    break;
                case RoleType.Medic:
                    // 治疗
                    magicIds = [1012, 1044, 1045];
                    values = [(0, 0, 39), (0, 0, 40), (0, 0, 41)];
                    m = FunGameService.GenerateMagicCardPack(3, QualityType.Red, magicIds, values);
                    if (m != null) character.Equip(m);
                    w = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 11578);
                    if (w != null) character.Equip(w);
                    a = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 12525);
                    if (a != null) character.Equip(a);
                    s = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 13523);
                    if (s != null) character.Equip(s);
                    ac1 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14548);
                    if (ac1 != null) character.Equip(ac1);
                    ac2 = FunGameConstant.Equipment.FirstOrDefault(i => i.Id == 14548);
                    if (ac2 != null) character.Equip(ac2);
                    break;
                default:
                    break;
            }
            foreach (Skill magic in character.Skills)
            {
                if (magic.IsMagic) magic.Level = 8;
            }
        }
    }
}
