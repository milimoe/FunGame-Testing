using Milimoe.FunGame.Testing.Tests;

bool printout = false;
List<string> strs = FunGameSimulation.StartGame(printout);
if (printout == false)
{
    foreach (string str in strs)
    {
        Console.WriteLine(str);
    }
}


Console.ReadKey();
