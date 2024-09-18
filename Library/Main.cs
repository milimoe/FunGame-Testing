using Milimoe.FunGame.Testing.Tests;

FunGameSimulation.LoadModules();

bool printout = true;
List<string> strs = FunGameSimulation.StartGame(printout);
if (printout == false)
{
    foreach (string str in strs)
    {
        Console.WriteLine(str);
    }
}

Console.ReadKey();
