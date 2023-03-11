using Microsoft.EntityFrameworkCore;
using MoviesApp;
using Npgsql;
using NpgsqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
            do
            {
                Console.WriteLine("Reinit db? (y/n/exit)");
                string? command = Console.ReadLine();

                if (command == "y")
                {
                    var parser = new MovieParser();
                    parser.Parse();

                    //DbInit.Init(parser); 
                    break;
                }
                if (command == "n")
                {
                    break;
                }
                if (command == "exit")
                {
                    Environment.Exit(0);
                }

                Console.WriteLine("Invalid command\n");
            } while (true);

            var qb = new QueryBuilder();
            do
            {                
                Console.WriteLine("Entry type? (m = movie/p = person/t = tag/exit)");
                string? entry = Console.ReadLine();

                if (entry == "exit") { Environment.Exit(0); }
                if (entry == "m" || entry == "p" || entry == "t")
                {
                    Console.WriteLine("Name?");
                    string? name = Console.ReadLine();

                    if (entry == "m")
                    {
                        if (name != null)
                            Console.WriteLine(qb.MovieQuery(name));
                    }
                    if (entry == "p")
                    {
                        if (name != null)
                            Console.WriteLine(qb.PersonQuery(name));
                    }
                    if (entry == "t")
                    {
                        if (name != null)
                            Console.WriteLine(qb.TagQuery(name));
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command\n");
                }
            } while (true);
        }

    }
}