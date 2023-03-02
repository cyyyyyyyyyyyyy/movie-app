using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    public class Category
    {
        public int categoryId { get; set; }
        public Movie movie { get; set; }
        public Person person { get; set; }
        public string category { get; set; }
        public Category() { }
    }
}
