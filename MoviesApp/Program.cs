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
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            MovieParser.Parse();

            //sw.Stop();
            //Console.WriteLine($"Time elapsed: {sw.Elapsed}");
        }
    }
}