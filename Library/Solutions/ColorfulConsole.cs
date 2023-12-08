using System;

namespace Milimoe.FunGame.Testing.Solutions
{
    class ColorfulConsole
    {
        public ColorfulConsole()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[System]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Core]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Api]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[Interface]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[DataRequest]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[Plugin]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[GameMode]");
            Console.ResetColor();
        }
    }
}
