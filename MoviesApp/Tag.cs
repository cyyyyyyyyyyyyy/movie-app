using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    public class Tag
    {
        public int tagId { get; set; }
        public string name { get; set; }
        public List<Movie> movies { get; set; }

        public Tag() { name = ""; tagId = 0; movies = new(); }
    }
}
