using Oshima.FunGame.OshimaModules;
using Oshima.FunGame.OshimaServers.Service;
using Application = System.Windows.Application;

namespace Milimoe.FunGame.Testing.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            CharacterModule cm = new();
            cm.Load();
            SkillModule sm = new();
            sm.Load();
            ItemModule im = new();
            im.Load();
            FunGameService.InitFunGame();
            FunGameSimulation.InitFunGameSimulation();
        }
    }
}
