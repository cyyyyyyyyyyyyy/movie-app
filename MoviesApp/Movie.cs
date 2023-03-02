namespace MoviesApp
{
    public class Movie
    {
        public string movieId { get; set; }
        //public string mainTitle { get; set; }
        public List<Title> titles { get; set; }
        public List<Movie> top10 { get; set; }
        //public List<Person> persons { get; set; }
        //public Director director { get; set; }
        public List<Category> categories { get; set; }
        public List<Tag> tags { get; set; }
        public double rating { get; set; }
        public Movie()
        {
            movieId = ""; /*mainTitle = ""*/; titles = new(); /*persons = new();*/ top10 = new();
            /*director = new()*/ categories = new(); tags = new(); rating = 0.0;
        }
    }

    public class Title
    {
        public int titleId { get; set; }
        public string title { get; set; }
        public Movie movie { get; set; }
        public Title() { titleId = 0; title = ""; movie = new Movie(); }
    }
}
