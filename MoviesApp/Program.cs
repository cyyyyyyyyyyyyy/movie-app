using MoviesApp;
using Npgsql;
using NpgsqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MoviesApp
{
    class MainClass
    {
        static void Main()
        {
            MainLoop();            
        }

        static void MainLoop()
        {
            MovieParser.Parse();
            do
            {
                Console.WriteLine("Reinit db? (y/n/exit)");
                string? command = Console.ReadLine();

                if (command == "y") { DbInit.Init(); break; }                    
                if (command == "n") { break; }
                if (command == "exit") { Environment.Exit(0); }                
                
                Console.WriteLine("Invalid command\n");
            } while (true);
        }
    }
}