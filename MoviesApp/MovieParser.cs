using System.Globalization;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MoviesApp
{
    public class MovieParser
    {
        private Dictionary<string, string> _actorsIdsNames = new(); // personid, personName

        private Dictionary<string, Dictionary<string, List<string>>> _movieNamesActorsList =
            new(); // filmName, (actor/director, personName)

        private Dictionary<string, List<string>> _actorNamesMovieList =
            new(); // actorName, movieList

        private Dictionary<int, string> _tagCodesNames = new();
        private Dictionary<string, double> _moviesIdsRating = new();
        private Dictionary<string, int> _moviesImdbIdsLensIds = new();
        private Dictionary<int, string> _moviesLensIdsImdbIds = new();

        private ConcurrentDictionary<int, List<int>> _moviesLensIdsTagsIds = new();
        private ConcurrentDictionary<int, List<int>> _TagsIdsMovieLensIds = new();

        ConcurrentDictionary<string, Movie> _movies = new(); // name, Movie
        ConcurrentDictionary<string, Person> _persons = new(); // name, List Movie
        ConcurrentDictionary<string, Tag> _tags = new(); // name, List Movie

        private string _currReleaseInfoPath = "alpha2.txt";

        //static private string _performanceInfoPath = "C:\\github\\movie-app\\MoviesApp\\performanceInfo\\";
        static private string standartroot = "C:\\github\\movie-app\\MoviesApp\\ml-latest\\";
        //private static string standartroot = "C:\\Users\\s-khechnev\\Desktop\\ml-latest\\";
        private string _movieCodesDir = standartroot + "MovieCodes_IMDB.tsv";
        private string _actorsCodesDir = standartroot + "ActorsDirectorsNames_IMDB.txt";
        private string _actorsAndMoviesCodesDir = standartroot + "ActorsDirectorsCodes_IMDB.tsv";

        private string _ratinggsDir = standartroot + "Ratings_IMDB.tsv";
        private string _linksDir = standartroot + "links_IMDB_MovieLens.csv";
        private string _tagCodesDir = standartroot + "TagCodes_MovieLens.csv";
        private string _tagScoresDir = standartroot + "TagScores_MovieLens.csv";

        public Dictionary<string, Movie> _movieIdMovie = new();
        private ConcurrentDictionary<string, Person> _personIdPerson = new();
        private ConcurrentDictionary<int, Tag> _tagIdTag = new();
        private ConcurrentDictionary<int, string> _lensImdb = new();
        //private int _movieId = 1;
        private int _titleId = 1;
        //private int _personId = 1;
        private int _categoryId = 1;

        public Dictionary<Person, List<Movie>> _personMovies = new();
        public Dictionary<Tag, List<Movie>> _tagMovies = new();

        delegate void parser();

        public void Parse()
        {
#if PARALLEL_PARSE
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
#else

            var parsers = new Dictionary<parser, string>
            {
                [ParseMovieCodes] = "ParseMovieCodes",
                [ParseActorsCodes] = "ParseActorsCodes",
                [ParseTagCodes] = "ParseTagCodes",

                [ParseRating] = "ParseRating",
                [ParseLinks] = "ParseLinks",
                [ParseTagScores] = "ParseTagScores",
                [ParseActorsAndMoviesCodes] = "ParseActorsAndMoviesCodes",
                [ComposeDictionaries] = "Compose Dictionaries",
                //[ComposeTop10] = "ComposeTop10"

                //[ComposeDictionaries] = "Compose Dictionaries"
            };

            //Stopwatch gw = new Stopwatch();
            //using (StreamWriter writer = File.AppendText(_performanceInfoPath + _currReleaseInfoPath))
            {
                //gw.Start();
                foreach (var fun in parsers)
                {
                    fun.Key();
                    //writer.WriteLine(fun.Value + "\t" + sw.Elapsed);                    
                }
                //gw.Stop();                
                //writer.WriteLine("Total\t" + gw.Elapsed);
                //writer.WriteLine();
            }
#endif

            AuxiliaryInfoDisposal();
        }

        private void AuxiliaryInfoDisposal()
        {
            //code
        }

        private void ParseMovieCodes()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            string? line = "";
            //Parallel.ForEach(File.ReadLines(_movieCodesDir), line =>
            using (StreamReader reader = File.OpenText(_movieCodesDir))
            {
                reader.ReadLine();
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

                    var span = line.AsSpan();
                    int index = span.IndexOf("\t");
                    string code = span.Slice(0, index).ToString();
                    string code1 = span.Slice(2, index - 2).ToString();
                    int titleCode = int.Parse(code1) * 10;

                    span = span.Slice(index + 1);
                    index = span.IndexOf("\t");
                    span = span.Slice(index + 1);

                    index = span.IndexOf("\t");
                    string name = span.Slice(0, index).ToString();
                    span = span.Slice(index + 1);

                    index = span.IndexOf("\t");
                    string region = span.Slice(0, index).ToString();
                    span = span.Slice(index + 1);

                    index = span.IndexOf("\t");
                    string lang = span.Slice(0, index).ToString();

                    bool crit = lang == "en" || lang == "ru" || region == "US"
                                || region == "RU" || region == "GB";

                    if (crit)
                    {
                        /*_moviesIdsNames.TryAdd(code, name);
                        if (!_moviesIdsTitles.TryAdd(code,
                            new List<Title>
                            {
                                new Title
                                {
                                    title = name,
                                    titleId = titleCode
                                } 
                            }
                            ))
                        {
                            _moviesIdsTitles[code].Add(new Title { title = name, titleId = titleCode + 1 });
                        }*/
                        if (!_movieIdMovie.ContainsKey(code))
                        {
                            var movie = new Movie();
                            movie.imdbId = code;
                            //movie.MovieId = _movieId;
                            //_movieId++;

                            _movieIdMovie.Add(code, movie);
                        }

                        var currMovie = _movieIdMovie[code];

                        var title = new Title() { titleId = _titleId, title = name, movie = currMovie };
                        _titleId++;

                        currMovie.titles.Add(title);
                    }

                    line = reader.ReadLine();
                }
            }

            parserSw.Stop();
            Console.WriteLine("ParseMovieCodes: " + parserSw.Elapsed);
        }

        private void ParseActorsCodes()
        {
            //Dictionary<string, string> actors = new Dictionary<string, string>();
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            Parallel.ForEach(File.ReadLines(_actorsCodesDir), line =>
            {
                /*string[] lineData = line.Split("\t");
                    string code = lineData[0];
                    string name = lineData[1];
                    */

                var span = line.AsSpan();
                int index = span.IndexOf("\t");
                string persId = span.Slice(0, index).ToString();
                span = span.Slice(index + 1);

                string name = span.Slice(0, span.IndexOf("\t")).ToString();

                _personIdPerson.TryAdd(persId, new Person() { /*Id = _personId,*/ personId = persId, name = name });
            });

            parserSw.Stop();
            Console.WriteLine("ParseActorCodes: " + parserSw.Elapsed);
        }

        private void ParseTagCodes()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            Parallel.ForEach(File.ReadLines(_tagCodesDir), line =>
            {
                /*
                    string[] lineData = line.Split(",");
                    string tagCode = lineData[0];
                    string tagName = lineData[1];
                    */

                var span = line.AsSpan();
                int index = span.IndexOf(",");
                string tagId = span.Slice(0, index).ToString();

                try
                {
                    int intTagId = int.Parse(tagId);
                    span = span.Slice(index + 1);

                    string tagName = span.ToString();

                    lock (_tagCodesNames)
                    {
                        //_tagCodesNames.TryAdd(intTagId, tagName);
                        _tagIdTag.TryAdd(intTagId, new Tag() { name = tagName, tagId = intTagId });
                    }
                }
                catch (FormatException)
                {
                    return;
                }
            });

            parserSw.Stop();
            Console.WriteLine("ParseTagCodes: " + parserSw.Elapsed);
        }

        private void ParseActorsAndMoviesCodes()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            Parallel.ForEach(File.ReadLines(_actorsAndMoviesCodesDir), line =>
            {
                /*
                    string[] lineData = line.Split("\t");
                    string movId = lineData[0];
                    string actId = lineData[2];
                    string cat = lineData[3];
                    */

                var span = line.AsSpan();
                int index = span.IndexOf("\t");
                string movId = span.Slice(0, index).ToString();
                span = span.Slice(index + 1);
                span = span.Slice(span.IndexOf("\t") + 1);

                index = span.IndexOf("\t");
                string actId = span.Slice(0, index).ToString();
                span = span.Slice(index + 1);

                string cat = span.Slice(0, span.IndexOf('\t')).ToString();

                if ((cat == "actress") || (cat == "self"))
                    cat = "actor";

                bool crit = (cat == "actor" || cat == "director") && _movieIdMovie.ContainsKey(movId);

                if (crit && _personIdPerson.ContainsKey(actId))
                {
                    var currMovie = _movieIdMovie[movId];

                    Category category = new Category()
                    {
                        /*categoryId = _categoryId,*/ category = cat, movie = currMovie
                    };

                    category.person = _personIdPerson[actId];

                    //_categoryId++;

                    lock (currMovie)
                    {
                        currMovie.categories.Add(category);
                    }
                }

                /*if (crit)
                {
                    lock (_movieNamesActorsList)
                    {
                        if (!_movieNamesActorsList.TryAdd(movId, new Dictionary<string, List<string>>()))
                        {
                            if (!_movieNamesActorsList[movId].TryAdd(cat, new List<string>
                                {
                                    actId
                                }))
                            {
                                if (cat == "actor")
                                    //lock (_movieNamesActorsList[movId][cat])
                                {
                                    _movieNamesActorsList[movId][cat].Add(actId);
                                }
                            }
                        }
                        else
                        {
                            //_movieNamesActorsList.Add(_moviesIdsNames[movId], new Dictionary<string, List<string>>());
                            _movieNamesActorsList[movId].TryAdd(cat, new List<string>
                            {
                                actId
                            });
                        }
                    }

                    lock (_actorNamesMovieList)
                    {
                        if (!_actorNamesMovieList.TryAdd(actId, new List<string> { movId }))
                        {
                            //lock (_actorNamesMovieList[actId])
                            {
                                _actorNamesMovieList[actId].Add(movId);
                            }
                        }
                    }
                }*/
            });

            parserSw.Stop();
            Console.WriteLine("ParseActorsAndMoviesCodes: " + parserSw.Elapsed);
        }

        private void ParseRating()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            Parallel.ForEach(File.ReadLines(_ratinggsDir), line =>
            {
                /*
                    string[] lineData = line.Split("\t");
                    string movId = lineData[0];
                    double rating = double.Parse(lineData[1], CultureInfo.InvariantCulture);
                    */

                var span = line.AsSpan();
                int index = span.IndexOf("\t");
                string movId = span.Slice(0, index).ToString();
                span = span.Slice(index + 1);

                index = span.IndexOf("\t");
                try
                {
                    double rating = double.Parse(span.Slice(0, index).ToString(), CultureInfo.InvariantCulture);
                    if (_movieIdMovie.ContainsKey(movId))
                        lock (_movieIdMovie)
                        {
                            _movieIdMovie[movId].rating = rating;
                        }
                }
                catch (FormatException)
                {
                    return;
                }
            });

            parserSw.Stop();
            Console.WriteLine("ParseRating: " + parserSw.Elapsed);
        }

        private void ParseLinks()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            Parallel.ForEach(File.ReadLines(_linksDir), line =>
            {
                /*
                    string[] lineData = line.Split(",");
                    string movLensId = lineData[0];
                    string imdbId = "tt" + lineData[1];
                    */

                var span = line.AsSpan();
                int index = span.IndexOf(",");
                string movLensId = span.Slice(0, index).ToString();

                try
                {
                    int intMovLensId = int.Parse(movLensId);
                    span = span.Slice(index + 1);

                    string imdbId = "tt" + span.Slice(0, span.IndexOf(",")).ToString();

                    /*if (_movieIdMovie.ContainsKey(imdbId))
                    {
                        lock (_moviesLensIdsImdbIds)
                        lock (_moviesImdbIdsLensIds)
                        {
                            // если один сработает а другой нет, то что? че я высрал))
                            _moviesLensIdsImdbIds.TryAdd(intMovLensId, imdbId);
                            _moviesImdbIdsLensIds.TryAdd(imdbId, intMovLensId);
                        }
                    }*/
                    _lensImdb.TryAdd(intMovLensId, imdbId);
                }
                catch (FormatException)
                {
                    return;
                }
            });

            parserSw.Stop();
            Console.WriteLine("ParseLinks: " + parserSw.Elapsed);
        }

        private void ParseTagScores()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            //string? line = "";
            //using (StreamReader reader = File.OpenText(_tagScoresDir))
            Parallel.ForEach(File.ReadLines(_tagScoresDir), line =>
            {
                var span = line.AsSpan();
                int index = span.IndexOf(",");
                string movLensId = span.Slice(0, index).ToString();
                if (movLensId == "movieId") return;
                int intMovLensId = int.Parse(movLensId);
                span = span.Slice(index + 1);

                index = span.IndexOf(",");
                string tagId = span.Slice(0, index).ToString();
                int intTagId = int.Parse(tagId);
                span = span.Slice(index + 1);

                double relevance = double.Parse(span.ToString(), CultureInfo.InvariantCulture);

                /*if (_moviesLensIdsImdbIds.ContainsKey(intMovLensId) && (relevance > 0.5))
                {
                    if (!_moviesLensIdsTagsIds.TryAdd(intMovLensId, new List<int> { intTagId }))
                        lock (_moviesLensIdsTagsIds[intMovLensId])
                        {
                            _moviesLensIdsTagsIds[intMovLensId].Add(intTagId);
                        }

                    if (!_TagsIdsMovieLensIds.TryAdd(intTagId, new List<int> { intMovLensId }))
                        lock (_moviesLensIdsTagsIds[intMovLensId])
                        {
                            _TagsIdsMovieLensIds[intTagId].Add(intMovLensId);
                        }
                }*/

                if (relevance > 0.5)
                {
                    var movId = _lensImdb[intMovLensId];

                    if (_movieIdMovie.ContainsKey(movId))
                    {
                        var currMovie = _movieIdMovie[movId];
                        lock (_tagCodesNames)
                        {
                            currMovie.tags.Add(_tagIdTag[intTagId]);
                        }
                    }
                }
            });

            parserSw.Stop();
            Console.WriteLine("ParseTagScores: " + parserSw.Elapsed);
        }

        private void ComposeDictionaries()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            foreach (var movie in _movieIdMovie.Values)
            {
                foreach (var person in movie.categories.Select(item => item.person))
                {
                    if (_personMovies.ContainsKey(person))
                    {
                        _personMovies[person].Add(movie);
                    }
                    else
                    {
                        _personMovies.Add(person, new List<Movie>() { movie });
                    }
                }

                foreach (var tag in movie.tags)
                {
                    if (_tagMovies.ContainsKey(tag))
                    {
                        _tagMovies[tag].Add(movie);
                    }
                    else
                    {
                        _tagMovies.Add(tag, new List<Movie>() { movie });
                    }
                }
            }

            parserSw.Stop();
            Console.WriteLine("ComposeDictionaries: " + parserSw.Elapsed);
        }

        [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
        private void ComposeTop10()
        {
            int count = 0;
            Parallel.ForEach(_movieIdMovie.Values, movie =>
            {
                //Dictionary<double, HashSet<Movie>> estimatedMovies = new();

                HashSet<Movie> addedMovies = new();

                if (movie.categories.Count != 0)
                {
                    foreach (var item in movie.categories)
                    {
                        var personMovies = _personMovies[item.person];

                        foreach (var mov in personMovies)
                        {
                            if (addedMovies.Contains(mov) || mov.imdbId == movie.imdbId)
                                continue;

                            var estimation = movie.GetEstimation(mov);

                            mov.temp = estimation;

                            addedMovies.Add(mov);
                        }
                    }
                }

                if (movie.tags.Count != 0)
                {
                    foreach (var item in movie.tags)
                    {
                        var tagMovies = _tagMovies[item];

                        foreach (var mov in tagMovies)
                        {
                            if (addedMovies.Contains(mov) || mov.imdbId == movie.imdbId)
                                continue;

                            var estimation = movie.GetEstimation(mov);

                            mov.temp = estimation;

                            addedMovies.Add(mov);
                        }
                    }
                }

                if (addedMovies.Count == 0)
                    return;

                int k = 0;

                foreach (var item in addedMovies.OrderByDescending(item => item.temp))
                {
                    movie.top10.Add(item);
                    k++;
                    if (k == 10)
                        break;
                }

                count++;
                if (count % 10000 == 0)
                {
                    Console.WriteLine(count);
                }
            });
        }

        /*private void ComposeDictionaries()
        {
            Stopwatch parserSw = new Stopwatch();
            parserSw.Start();

            var t1 = Task.Factory.StartNew(() => ComposeMovies());
            t1.Wait();
            var t2 = Task.Factory.StartNew(() => ComposeActors());
            t2.Wait();
            var t3 = Task.Factory.StartNew(() => ComposeTags());
            t3.Wait();
            //Task.WaitAll(new Task[] { t2, t3 });

            parserSw.Stop();
            Console.WriteLine("ComposeDictionaries: " + parserSw.Elapsed);
        }

        private void ComposeMovies()
        {
            Parallel.ForEach(_moviesIdsNames.Keys, movId =>
            {
                if (!_movies.ContainsKey(_moviesIdsNames[movId]))
                {
                    Movie movie = new Movie();
                    movie.ImdbId = movId;
                    movie.titles = _moviesIdsTitles[movId];

                    List<Person> newActors = new();
                    List<Person> existingActors = new();
                    Person? newDirector = null;
                    Person? existingDirector = null;
                    List<Tag> newTags = new();
                    List<Tag> existingTags = new();

                    if (_movieNamesActorsList.ContainsKey(movId))
                    {
                        if (_movieNamesActorsList[movId].ContainsKey("actor"))
                            foreach (string actorId in _movieNamesActorsList[movId]["actor"])
                            {
                                if (!_persons.ContainsKey(_actorsIdsNames[actorId]))
                                {
                                    Person actor = new Person
                                    {
                                        personId = actorId,
                                        name = _actorsIdsNames[actorId],
                                        //movies = new List<Movie> { movie }
                                    };
                                    Category cat = new Category { movie = movie, person = actor, category = "actor" };
                                    actor.categories.Add(cat);
                                    movie.categories.Add(cat);
                                    //movie.persons.Add(actor);  

                                    newActors.Add(actor);
                                }
                                else
                                {
                                    Person actor = _persons[_actorsIdsNames[actorId]];
                                    existingActors.Add(actor);
                                    //movie.persons.Add(actor);
                                }
                            }

                        if (_movieNamesActorsList[movId].ContainsKey("director"))
                        {
                            string directorId = _movieNamesActorsList[movId]["director"][0];
                            if (!_persons.ContainsKey(_actorsIdsNames[directorId]))
                            {
                                if (_movieNamesActorsList[movId].ContainsKey("actor"))
                                {
                                    if (_movieNamesActorsList[movId]["actor"].Contains(directorId))
                                    {
                                        bool found = false;
                                        foreach (var item in newActors)
                                        {
                                            if (item.personId == directorId)
                                            {
                                                Category cat = new Category
                                                    { movie = movie, person = item, category = "director" };
                                                item.categories.Add(cat);
                                                movie.categories.Add(cat);

                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            foreach (var item in existingActors)
                                            {
                                                if (item.personId == directorId)
                                                {
                                                    Category cat = new Category
                                                        { movie = movie, person = item, category = "director" };
                                                    item.categories.Add(cat);
                                                    movie.categories.Add(cat);

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    newDirector = new Person
                                    {
                                        personId = directorId,
                                        name = _actorsIdsNames[directorId],
                                        //movies = new List<Movie> { movie }
                                    };
                                    Category cat = new Category
                                        { movie = movie, person = newDirector, category = "director" };
                                    newDirector.categories.Add(cat);
                                    movie.categories.Add(cat);
                                    //movie.persons.Add(newDirector);         
                                }
                            }
                            else
                            {
                                existingDirector = _persons[_actorsIdsNames[directorId]];
                                //movie.persons.Add(existingDirector);
                            }
                        }
                    }

                    if (_moviesIdsRating.ContainsKey(movId))
                        movie.rating = _moviesIdsRating[movId];

                    if (_moviesImdbIdsLensIds.ContainsKey(movId))
                        if (_moviesLensIdsTagsIds.ContainsKey(_moviesImdbIdsLensIds[movId]))
                            foreach (int tagId in _moviesLensIdsTagsIds[_moviesImdbIdsLensIds[movId]])
                            {
                                if (!_tags.ContainsKey(_tagCodesNames[tagId]))
                                {
                                    Tag newTag = new Tag
                                    {
                                        tagId = tagId,
                                        name = _tagCodesNames[tagId],
                                        movies = new List<Movie> { movie }
                                    };

                                    movie.tags.Add(newTag);
                                    newTags.Add(newTag);
                                }
                                else
                                {
                                    Tag existingTag = _tags[_tagCodesNames[tagId]];
                                    existingTags.Add(existingTag);
                                    movie.tags.Add(existingTag);
                                }
                            }

                    foreach (var item in newActors)
                        _persons.TryAdd(item.name, item);

                    foreach (var item in existingActors)
                    {
                        Category cat = new Category { movie = movie, person = item, category = "actor" };
                        movie.categories.Add(cat);
                        lock (_persons[item.name].categories)
                            _persons[item.name].categories.Add(cat);
                        //_persons[item.name].movies.Add(movie);                                                    
                    }

                    if (newDirector != null)
                        _persons.TryAdd(newDirector.name, newDirector);

                    if (existingDirector != null)
                    {
                        Category cat = new Category { movie = movie, person = existingDirector, category = "director" };
                        movie.categories.Add(cat);
                        lock (_persons[existingDirector.name])
                        {
                            //_persons[existingDirector.name].movies.Add(movie);
                            _persons[existingDirector.name].categories.Add(cat);
                        }
                    }

                    foreach (var item in newTags)
                        _tags.TryAdd(item.name, item);

                    foreach (var item in existingTags)
                        lock (_tags[item.name].movies)
                            _tags[item.name].movies.Add(movie);

                    _movies.TryAdd(_moviesIdsNames[movId], movie);
                }
            });
        }

        private void ComposeActors()
        {
            Parallel.ForEach(_actorsIdsNames.Keys, actId =>
            {
                if (!_persons.ContainsKey(_actorsIdsNames[actId]))
                {
                    Person actor = new Person();
                    actor.personId = actId;
                    actor.name = _actorsIdsNames[actId];

                    actor.categories = new List<Category>();
                    //List<Movie> movies = new List<Movie>();

                    /*if (_actorNamesMovieList.ContainsKey(actId))
                        foreach (string movieId in _actorNamesMovieList[actId])
                            if (_movies.ContainsKey(_moviesIdsNames[movieId]))
                                if (_movies[_moviesIdsNames[movieId]].actors.Count != 0)
                                    movies.Add(_movies[_moviesIdsNames[movieId]]);
                    #1#

                    //actor.movies = movies;

                    _persons.TryAdd(_actorsIdsNames[actId], actor);
                }
            });
        }

        private void ComposeTags()
        {
            Parallel.ForEach(_tagCodesNames.Keys, tagId =>
            {
                if (!_tags.ContainsKey(_tagCodesNames[tagId]))
                {
                    Tag tag = new Tag();
                    tag.tagId = tagId;
                    tag.name = _tagCodesNames[tagId];

                    List<Movie> movies = new List<Movie>();

                    /*if (_TagsIdsMovieLensIds.ContainsKey(tagId))
                        foreach (string movieId in _TagsIdsMovieLensIds[tagId])
                            if (_movies.ContainsKey(_moviesIdsNames[_moviesLensIdsImdbIds[movieId]]))
                                if (_movies[_moviesIdsNames[_moviesLensIdsImdbIds[movieId]]].movieId 
                                     == _moviesLensIdsImdbIds[movieId])
                                    movies.Add(_movies[_moviesIdsNames[_moviesLensIdsImdbIds[movieId]]]);
                    #1#
                    tag.movies = movies;

                    _tags.TryAdd(_tagCodesNames[tagId], tag);
                }
            });
        }*/
    }
}