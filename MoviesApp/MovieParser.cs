using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;

namespace MoviesApp
{
    public class MovieParser
    {
        static private Dictionary<string, string> _moviesIdsNames = new Dictionary<string, string>(); // id, name
        static private Dictionary<string, string> _actorsIdsNames = new Dictionary<string, string>(); // id, name

        static private Dictionary<string, Dictionary<string, List<string>>> _movieNamesActorsList =
            new Dictionary<string, Dictionary<string, List<string>>>(); // filmName, (actor/director, personName)
        static private Dictionary<string, List<string>> _actorNamesMovieList =
            new Dictionary<string, List<string>>();

        static private Dictionary<string, string> _tagCodesNames = new Dictionary<string, string>();
        static private Dictionary<string, double> _moviesIdsRating = new Dictionary<string, double>();   
        static private Dictionary<string, string> _moviesImdbIdsLensIds = new Dictionary<string, string>();
        static private Dictionary<string, string> _moviesLensIdsImdbIds = new Dictionary<string, string>();

        static private Dictionary<string, List<string>> _moviesLensIdsTagsIds = new Dictionary<string, List<string>>();
        //static private Dictionary<string, List<string>> _TagsIdsMovieLensIds = new Dictionary<string, List<string>>();

        static private Dictionary<string, Movie> _movies = new Dictionary<string, Movie>(); // name, Movie
        static private Dictionary<string, List<Movie>> _actors = new Dictionary<string, List<Movie>>(); // name, List Movie
        static private Dictionary<string, List<Movie>> _tags = new Dictionary<string, List<Movie>>();

        static private string _currReleaseInfoPath = "alpha1.txt";

        static private string _performanceInfoPath = "C:\\proga\\csharp\\[github]MoviesApp - outdated\\MoviesApp\\performanceInfo\\";
        static private string standartroot = "C:\\proga\\csharp\\[github]MoviesApp - outdated\\MoviesApp\\ml-latest\\";
        static private string _movieCodesDir = standartroot + "MovieCodes_IMDB.tsv";
        static private string _actorsCodesDir = standartroot + "ActorsDirectorsNames_IMDB.txt";
        static private string _actorsAndMoviesCodesDir = standartroot + "ActorsDirectorsCodes_IMDB.tsv";

        static private string _ratinggsDir = standartroot + "Ratings_IMDB.tsv";
        static private string _linksDir = standartroot + "links_IMDB_MovieLens.csv";
        static private string _tagCodesDir = standartroot + "TagCodes_MovieLens.csv";
        static private string _tagScoresDir = standartroot + "TagScores_MovieLens.csv";

        delegate void parser();
        public static void Parse()
        {
            var t1 = Task.Factory.StartNew(() => ParseMovieCodes());
            var t2 = t1.ContinueWith(ant => ParseRating());
            var t3 = t1.ContinueWith(ant => ParseLinks());
            var t4 = t3.ContinueWith(ant => ParseTagScores());

            var t5 = Task.Factory.StartNew(() => ParseActorsCodes());
            var t6 = Task.Factory.StartNew(() => ParseTagCodes());

            var t7 = Task.Factory.ContinueWhenAll(new Task[2] { t1, t5 }, ant => ParseActorsAndMoviesCodes());

            Task.WaitAll(new Task[] { t2, t4, t6, t7 });
            var t8 = Task.Factory.StartNew(() => ComposeDictionaries());
            t8.Wait();

            /*var parsers = new Dictionary<parser, string>
            {
                [ParseMovieCodes] = "ParseMovieCodes",
                [ParseActorsCodes] = "ParseActorsCodes",
                [ParseTagCodes] = "ParseTagCodes",

                [ParseRating] = "ParseRating",
                [ParseLinks] = "ParseLinks",
                [ParseTagScores] = "ParseTagScores",
                [ParseActorsAndMoviesCodes] = "ParseActorsAndMoviesCodes",

                [ComposeDictionaries] = "Compose Dictionaries"
            };

            Stopwatch gw = new Stopwatch();
            Stopwatch sw = new Stopwatch();
            using (StreamWriter writer = File.AppendText(_performanceInfoPath + _currReleaseInfoPath))
            {
                gw.Start();
                foreach (var fun in parsers)
                {
                    sw.Start();
                    fun.Key();
                    sw.Stop();
                    Console.WriteLine(fun.Value + ": elapsed - " + sw.Elapsed);
                    writer.WriteLine(fun.Value + "\t" + sw.Elapsed);
                    sw.Reset();
                }
                gw.Stop();
                Console.WriteLine("Total: elapsed - " + gw.Elapsed);
                writer.WriteLine("Total\t" + gw.Elapsed);
                writer.WriteLine();
            }*/
        }

        public static void ParseMovieCodes()
        {
            //Dictionary<string, string> movies = new Dictionary<string, string>();
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();
            string? line = "";

            using (StreamReader reader = File.OpenText(_movieCodesDir))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                while (line != null)
                {
                    /*
                    string[] lineData = line.Split("\t");
                    string code = lineData[0];
                    string name = lineData[2];
                    string region = lineData[3];
                    string lang = lineData[4];
                    */

                    
                    string code = line.Substring(0, line.IndexOf("\t"));

                    var span = line.AsSpan().Slice(line.IndexOf("\t") + 1);
                    span = span.Slice(span.IndexOf("\t") + 1);

                    string name = span.Slice(0, span.IndexOf("\t")).ToString();
                    span = span.Slice(span.IndexOf("\t") + 1);

                    string region = span.Slice(0, span.IndexOf("\t")).ToString();
                    span = span.Slice(span.IndexOf("\t") + 1);

                    string lang = span.Slice(0, span.IndexOf("\t")).ToString();
                    

                    bool crit = lang == "en" || lang == "ru" || region == "US"
                        || region == "RU" || region == "GB";
                    if (crit)
                    {
                        _moviesIdsNames.TryAdd(code, name);
                    }


                    line = reader.ReadLine();
                }
            }

            //_moviesIdsNames = movies;
            parserSw.Stop();
            Console.WriteLine("ParseMovieCodes: " + parserSw.Elapsed);
        }

        static public void ParseActorsCodes()
        {
            //Dictionary<string, string> actors = new Dictionary<string, string>();
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();
            string? line = "";

            using (StreamReader reader = File.OpenText(_actorsCodesDir))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                while (line != null)
                {
                    /*string[] lineData = line.Split("\t");
                    string code = lineData[0];
                    string name = lineData[1];
                    */
                    
                    string code = line.Substring(0, line.IndexOf("\t"));
                    var span = line.AsSpan().Slice(line.IndexOf("\t") + 1);
                    string name = span.Slice(0, span.IndexOf("\t")).ToString();
                    

                    _actorsIdsNames.TryAdd(code, name);

                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseActorCodes: " + parserSw.Elapsed);
        }

        static public void ParseTagCodes()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();
            string? line = "";

            using (StreamReader reader = File.OpenText(_tagCodesDir))
            {
                reader.ReadLine();
                line = reader.ReadLine();

                while (line != null)
                {
                    /*
                    string[] lineData = line.Split(",");
                    string tagCode = lineData[0];
                    string tagName = lineData[1];
                    */
                    
                    string tagCode = line.Substring(0, line.IndexOf(","));
                    var span = line.AsSpan().Slice(line.IndexOf(",") + 1);
                    string tagName = span.Slice(0).ToString();
                    

                    _tagCodesNames.TryAdd(tagCode, tagName);

                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseTagCodes: " + parserSw.Elapsed);
        }

        static public void ParseActorsAndMoviesCodes()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();
            string? line = "";

            using (StreamReader reader = File.OpenText(_actorsAndMoviesCodesDir))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                //string prevMovie = "";
                //bool keyUniqueness = true;

                while (line != null)
                {
                    /*
                    string[] lineData = line.Split("\t");
                    string movId = lineData[0];
                    string actId = lineData[2];
                    string cat = lineData[3];
                    */
                    
                    string movId = line.Substring(0, line.IndexOf("\t"));
                    var span = line.AsSpan().Slice(line.IndexOf("\t") + 1);
                    span = span.Slice(span.IndexOf("\t") + 1);

                    string actId = span.Slice(0, span.IndexOf("\t")).ToString();
                    span = span.Slice(span.IndexOf("\t") + 1);

                    string cat = span.Slice(0, span.IndexOf('\t')).ToString();
                    

                    if ((cat == "actress") || (cat == "self"))
                        cat = "actor";

                    bool crit = (cat == "actor" || cat == "director")

                        && _moviesIdsNames.ContainsKey(movId) 
                        && _actorsIdsNames.ContainsKey(actId);

                    if (crit)
                    {
                        if (!_movieNamesActorsList.TryAdd(movId, new Dictionary<string, List<string>>()))
                        {
                            if (!_movieNamesActorsList[movId].TryAdd(cat, new List<string> {
                                    actId }))
                            {
                                if (cat == "actor")
                                    _movieNamesActorsList[movId][cat].Add(actId);
                            }
                        }
                        else
                        {
                            //_movieNamesActorsList.Add(_moviesIdsNames[movId], new Dictionary<string, List<string>>());
                            _movieNamesActorsList[movId].Add(cat, new List<string>
                                {
                                    actId
                                });
                        }
                        if (!_actorNamesMovieList.TryAdd(actId, new List<string> { movId}))
                        {
                            _actorNamesMovieList[actId].Add(movId);
                        }
                    }

                    //prevMovie = movId;
                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseActorsAndMoviesCodes: " + parserSw.Elapsed);
        }

        static public void ParseRating()
        {
            //Dictionary<string, float> moviesIdsRating = new Dictionary<string, float>();
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();
            string? line = "";

            using (StreamReader reader = File.OpenText(_ratinggsDir))
            {
                reader.ReadLine();
                line = reader.ReadLine();
                
                while (line != null)
                {
                    /*
                    string[] lineData = line.Split("\t");
                    string movId = lineData[0];
                    double rating = double.Parse(lineData[1], CultureInfo.InvariantCulture);
                    */
                    
                    string movId = line.Substring(0, line.IndexOf("\t"));
                    var span = line.AsSpan().Slice(line.IndexOf("\t") + 1);
                    double rating = double.Parse(span.Slice(0, span.IndexOf("\t")).ToString(), CultureInfo.InvariantCulture);
                    

                    if (_moviesIdsNames.ContainsKey(movId))
                        _moviesIdsRating.TryAdd(movId, rating);

                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseRating: " + parserSw.Elapsed);
        }

        static public void ParseLinks()
        {
            //Dictionary<string, string> linksIds = new Dictionary<string, string>();
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            string? line = "";
            using (StreamReader reader = File.OpenText(_linksDir))
            {
                reader.ReadLine();
                line = reader.ReadLine();

                while (line != null)
                {
                    /*
                    string[] lineData = line.Split(",");
                    string movLensId = lineData[0];
                    string imdbId = "tt" + lineData[1];
                    */
                    
                    string movLensId = line.Substring(0, line.IndexOf(","));
                    var span = line.AsSpan().Slice(line.IndexOf(",") + 1);
                    string imdbId = "tt" + span.Slice(0, span.IndexOf(",")).ToString();
                    

                    if (_moviesIdsNames.ContainsKey(imdbId))
                    {
                        // если один сработает а другой нет, то что?
                        _moviesLensIdsImdbIds.TryAdd(movLensId, imdbId);
                        _moviesImdbIdsLensIds.TryAdd(imdbId, movLensId);
                    }                        

                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseLinks: " + parserSw.Elapsed);
        }

        static public void ParseTagScores()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();
            string? line = "";

            using (StreamReader reader = File.OpenText(_tagScoresDir))
            {
                reader.ReadLine();
                line = reader.ReadLine();
                //reader.

                while (line != null)
                {
                    /*
                    string[] lineData = line.Split(",");
                    string movLensId = lineData[0];
                    string tagId = lineData[1];
                    double relevance = double.Parse(lineData[2], CultureInfo.InvariantCulture);
                    */
                    
                    string movLensId = line.Substring(0, line.IndexOf(","));
                    var span = line.AsSpan().Slice(line.IndexOf(",") + 1);
                    string tagId = span.Slice(0, span.IndexOf(",")).ToString();

                    span = span.Slice(span.IndexOf(",") + 1);
                    double relevance = double.Parse(span.Slice(0).ToString(), CultureInfo.InvariantCulture);
                    

                    if (_moviesLensIdsImdbIds.ContainsKey(movLensId) && (relevance > 0.5))
                        if (!_moviesLensIdsTagsIds.TryAdd(movLensId, new List<string> { tagId }))
                        {
                            _moviesLensIdsTagsIds[movLensId].Add(tagId);

                        }

                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseTagScores: " + parserSw.Elapsed);
        }

        static public void ComposeDictionaries()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            var t1 = Task.Factory.StartNew( () => ComposeMovies());
            t1.Wait();
            var t2 = Task.Factory.StartNew( () => ComposeActors());            
            t2.Wait();

            parserSw.Stop();
            Console.WriteLine("ComposeDictionaries: " + parserSw.Elapsed);
        }

        static public void ComposeMovies()
        {
            Parallel.ForEach(_moviesIdsNames.Keys, movId =>
            {
            //foreach (string movId in _moviesIdsNames.Keys
            //{
                lock (_movies)
                {
                    if (!_movies.ContainsKey(_moviesIdsNames[movId]))
                    {
                        Movie movie = new Movie();
                        movie.id = movId;
                        movie.name = _moviesIdsNames[movId];

                        if (_movieNamesActorsList.ContainsKey(movId))
                        {
                            if (_movieNamesActorsList[movId].ContainsKey("actor"))
                                foreach (string actorId in _movieNamesActorsList[movId]["actor"])
                                    movie.actors.Add(_actorsIdsNames[actorId]);

                            if (_movieNamesActorsList[movId].ContainsKey("director"))
                                movie.director = _actorsIdsNames[_movieNamesActorsList[movId]["director"][0]];
                        }

                        if (_moviesIdsRating.ContainsKey(movId))
                            movie.rating = _moviesIdsRating[movId];

                        if (_moviesImdbIdsLensIds.ContainsKey(movId))
                            if (_moviesLensIdsTagsIds.ContainsKey(_moviesImdbIdsLensIds[movId]))
                                foreach(string tag in _moviesLensIdsTagsIds[_moviesImdbIdsLensIds[movId]])
                                    movie.tags.Add(_tagCodesNames[tag]);

                        _movies.Add(_moviesIdsNames[movId], movie);
                    }
                }
                
            //}
            });
        }

        static public void ComposeActors()
        {
            Parallel.ForEach(_actorsIdsNames.Keys, actId =>
            {
                lock (_actors)
                {
                    if (!_actors.ContainsKey(_actorsIdsNames[actId]))
                    {
                        //Actor actor = new Actor();
                        List<Movie> movies = new List<Movie>();

                        if (_actorNamesMovieList.ContainsKey(actId))
                            foreach (string movieId in _actorNamesMovieList[actId])
                                if (_movies.ContainsKey(_moviesIdsNames[movieId]))
                                    if (_movies[_moviesIdsNames[movieId]].id == movieId)
                                        movies.Add(_movies[_moviesIdsNames[movieId]]);

                        _actors.Add(_actorsIdsNames[actId], movies);
                    }
                }
            });
        }

        static public void ComposeTags()
        {
            Parallel.ForEach(_tagCodesNames.Keys, tagId =>
            {
                lock (_tags)
                {
                    if (!_tags.ContainsKey(_tagCodesNames[tagId]))
                    {
                        List<Movie> movies = new List<Movie>();

                        //if (_moviesLensIdsTagsIds[])
                    }
                }
            });
        }
    }
}
