using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApp
{
    public class Movie
    {
        [Key] 
        //public int MovieId { get; set; }

        public string imdbId { get; set; }

        //public string mainTitle { get; set; }
        public List<Title> titles { get; set; }

        public List<Movie> top10 { get; set; }

        //public List<Person> persons { get; set; }
        //public Director director { get; set; }
        public List<Category> categories { get; set; }
        //public List<Person> persons => categories.Select(item => item.person).ToList();
        public List<Tag> tags { get; set; }
        public double rating { get; set; }
        [NotMapped]
        public double temp { get; set; }

        public Movie()
        {
            imdbId = ""; /*mainTitle = ""*/
            titles = new(); /*persons = new();*/
            top10 = new();
            /*director = new()*/
            categories = new();
            tags = new();
            rating = 0.0;
        }

        public double GetEstimation(Movie other)
        {
            float personsEstimation;
            float tagsEstimation;

            if (other.categories.Count != 0 && categories.Count != 0)
            {
                var intersectionPersonCount = categories.Intersect(other.categories).Count();
                personsEstimation = (float)intersectionPersonCount / (4 * categories.Count);
            }
            else
            {
                personsEstimation = 0f;
            }

            if (other.tags.Count != 0 && tags.Count != 0)
            {
                var intersectionTagsCount = tags.Intersect(other.tags).Count();
                tagsEstimation = (float)intersectionTagsCount / (4 * tags.Count);
            }
            else
            {
                tagsEstimation = 0f;
            }

            return personsEstimation + tagsEstimation + other.rating / 20;
        }
    }

    public class Title
    {
        public int titleId { get; set; }
        public string title { get; set; }
        public Movie movie { get; set; }

        public Title()
        {
            titleId = 0;
            title = "";
            movie = new Movie();
        }
    }
}