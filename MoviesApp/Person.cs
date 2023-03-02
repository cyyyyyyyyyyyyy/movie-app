using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    public class Person
    {
        public string personId { get; set; }
        public string name { get; set; }
        //public List<Movie> movies { get; set; }
        public List<Category> categories { get; set; }
        public Person() { name = ""; personId = ""; /*movies = new();*/ categories = new(); }

        //public virtual string TypeToString() { return "Person"; }
    }
    /*public class Actor : Person
    {
        public Actor() : base() { }
        public override string TypeToString() { return "Actor"; }
    }
    public class Director : Person
    {
        public Director() : base() { }
        public override string TypeToString() { return "Director"; }
    }*/
}
