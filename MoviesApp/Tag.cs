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

        public Tag()
        {
            name = "";
            tagId = 0;
            movies = new();
        }

        public override bool Equals(object? obj)
        {
            if (obj is Tag)
            {
                return name == (obj as Tag).name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}