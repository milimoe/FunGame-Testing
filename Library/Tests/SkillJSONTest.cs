using Addons;

namespace Milimoe.FunGame.Testing.Tests
{
    internal class SkillJSONTest
    {
        public SkillJSONTest()
        {
            //Factory.CreateGameModuleEntityConfig<Item>(nameof(SkillJSONTest), nameof(Item), []);
            ExampleCharacterModule cm = new();
            cm.Load();
            ExampleItemModule im = new();
            im.Load();
            ExampleSkillModule sm = new();
            sm.Load();
            _ = new FunGameSimulation();
        }
    }
}
