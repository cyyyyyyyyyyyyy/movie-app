using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApp
{
    public class Movie
    {
        //public int MovieId { get; set; }
        [Key]
        public string imdbId { get; set; }
        public List<Title> titles { get; set; }
        public List<Movie> top10 { get; set; }
        public List<Category> categories { get; set; }
        public List<Tag> tags { get; set; }
        public double rating { get; set; }
        [NotMapped]
        public double temp { get; set; }

        public Movie()
        {
            imdbId = ""; 
            titles = new(); 
            top10 = new();
            categories = new();
            tags = new();
            rating = 0.0;
        }

        public double Estimate(Movie compMovie)
        {
            double estimateByPers;
            double estimateByTag;

            if (compMovie.categories.Count != 0 && categories.Count != 0)
                estimateByPers = (double)categories.Intersect(compMovie.categories).Count() / (5*categories.Count);                
            else
                estimateByPers = 0.0;            

            if (compMovie.tags.Count != 0 && tags.Count != 0)            
                estimateByTag = (float)tags.Intersect(compMovie.tags).Count() / (3 * tags.Count);            
            else            
                estimateByTag = 0.0;

            double finalEst = estimateByPers + estimateByTag + (compMovie.rating * (7 / 150));
            return finalEst;
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