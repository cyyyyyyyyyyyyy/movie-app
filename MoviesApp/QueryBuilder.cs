using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
    internal class QueryBuilder
    {
        internal string MovieQuery(string movName)
        {
            string? movieId;
            using (var context = new AppContext())
            {
                movieId = (from title in context.Titles.Where(t => t.title.ToLower() == movName.ToLower())
                                      //where title.title.ToLower() == movName.ToLower()
                                      .Include(t => t.movie)
                                      .AsSplitQuery()
                           select title.movie.imdbId).FirstOrDefault();

                if (movieId == null)
                {
                    return "No such movie was found";
                }
            }
            using (var context = new AppContext())
            {
                //foreach (string movid in movieId)
                {
                    var mov = (from m in context.Movies.Where(m => m.imdbId == movieId)
                               .Include(m => m.titles)
                               .Include(m => m.top10)
                                .ThenInclude(top => top.titles)
                               .Include(m => m.tags)
                               .Include(m => m.categories)
                                .ThenInclude(c => c.person)
                               //where m.imdbId == movieId
                               .AsSplitQuery()
                               select m).Single();

                    string outString = $"Movie Title: {mov.titles[0].title} (id = {mov.imdbId})";
                    outString += $"\tRating: {mov.rating}\n";
                    outString += "\tPersons: \n";
                    foreach (var c in mov.categories)
                        outString += $"\t\t{c.person.name} - {c.category}\n";
                    outString += "\tTags: \n";
                    foreach (var t in mov.tags)
                        outString += $"\t\t{t.name}\n";
                    outString += "\tTop10: \n";
                    foreach (var top in mov.top10)
                        outString += $"\t\t{top.titles[0].title}\n";

                    return outString;
                }
            }

            //return "No such movie was found";
        }

        internal string PersonQuery(string persName)
        {
            List<Person>? persList;
            using (var context = new AppContext())
            {
                persList = (from p in context.Persons
                            where p.name.ToLower() == persName.ToLower()
                            select p).ToList();
                //if (persList.Count() == 0)
                //return "No such person was found";
            }
            using (var context = new AppContext())
            {
                foreach (var pers in persList)
                {
                    var catList = (from c in context.Categories.Where(c => c.person.personId == pers.personId)
                                .Include(c => c.movie)
                                .ThenInclude(m => m.titles)
                                .Include(c => c.person)
                             //where c.person.personId == pers.personId
                             .AsSplitQuery()
                                   select c).ToList();

                    string outString = $"Person Name: {pers.name} (id = {pers.personId}\n)";
                    outString += "\tMovies: \n";
                    foreach (var c in catList)
                        outString += $"\t\t{c.movie.titles[0].title} - {c.category}\n";

                    return outString;
                }
            }

            return "No such person was found";
        }

        internal string TagQuery(string tagName)
        {
            using (var context = new AppContext())
            {
                var tag = (from t in context.Tags.Where(t => t.name == tagName)
                            .Include(t => t.movies)
                             .ThenInclude(m => m.titles)
                            //where t.name == tagName
                            .AsSplitQuery()
                           select t).FirstOrDefault();

                if (tag == null) return "No such tag was found\n";

                string outString = "Tag Name: " + tag.name + "(id = " + tag.tagId + ")\n";
                outString += "\tMovies: \n";
                foreach (var m in tag.movies)
                    outString += "\t\t" + m.titles[0].title + "\n";

                return outString;
            }

            //return "No such tag was found\n";
        }
    }

}
