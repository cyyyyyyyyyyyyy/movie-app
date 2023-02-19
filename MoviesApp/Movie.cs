using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    public class Movie
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> actors { get; set; }
        public string director { get; set; }
        public List<string> tags { get; set; }
        public double rating { get; set; }
        public Movie(string id, string name, List<string> actors, string director, List<string> tags, double rating)
        {
            this.id = id;
            this.name = name;
            this.actors = actors;
            this.director = director;
            this.tags = tags;
            this.rating = rating;
        }

        public Movie() 
        {
            id = "";
            name = "";
            actors = new List<string>();
            director = "";
            tags = new List<string>();
            rating = 0.0;
        }
    }
}
