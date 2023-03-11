using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    public class Person
    {
        //public int Id { get; set; }
        [Key]
        public string personId { get; set; }
        public string name { get; set; }
        //public List<Category> categories { get; set; }
        public Person()
        {
            name = "";
            personId = "";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Person)
            {
                return personId == (obj as Person).personId;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return personId.GetHashCode();
        }
    }
}