using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    public class Tag
    {
        public string name { get; set; }
        public string id { get; set; }
        public List<string> movies { get; set; }

        public Tag() { name = ""; id = ""; movies = new List<string>(); }
        public Tag(string name, string id, List<string> movies)
        {
            this.name = name;
            this.id = id;
            this.movies = movies;
        }
    }
}
