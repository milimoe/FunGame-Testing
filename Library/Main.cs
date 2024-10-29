using Oshima.Core.Utils;
using Oshima.FunGame.OshimaModules;

CharacterModule cm = new();
cm.Load();
SkillModule sm = new();
sm.Load();
ItemModule im = new();
im.Load();

FunGameSimulation.InitCharacter();
FunGameSimulation.StartGame(true, true);
