using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace MoviesApp
{
    public static class DbInit
    {
        public class AppContext: DbContext
        {
            public DbSet<Movie> Movies => Set<Movie>();
            public DbSet<Person> Actors => Set<Person>();
            public DbSet<Tag> Tags => Set<Tag>();
            
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql(
                "Host=localhost;" +
                "Port=5432;" +
                "Database=myFilmDb;" +
                "Username=postgres;" +
                "Password=12345");
            }
            
            protected override void OnModelCreating(ModelBuilder modelBuilder) 
            {
                modelBuilder.Entity<Movie>()
                .HasMany(m => m.top10)
                .WithMany();
            }
        }
        public static void Init() 
        {
            using (var context = new AppContext()) 
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            NpgsqlConnection connection = new NpgsqlConnection(
                "Host=localhost;" +
                "Port=5432;" +
                "Database=myFilmDb;" +
                "Username=postgres;" +
                "Password=12345"
            );

            connection.Open();
            using (var writer = connection.BeginBinaryImport("COPY \"movies\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in MovieParser._movies.Values) 
                {
                    writer.StartRow();
                    writer.Write(mov.movieId, NpgsqlDbType.Text);
                    //writer.Write(mov.director.personId, NpgsqlDbType.Text);
                    writer.Write(mov.rating, NpgsqlDbType.Double);
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"persons\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Person pers in MovieParser._persons.Values)
                {
                    writer.StartRow();
                    writer.Write(pers.personId, NpgsqlDbType.Text);
                    writer.Write(pers.name, NpgsqlDbType.Text);
                    //writer.Write(pers.TypeToString(), NpgsqlDbType.Text);
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"tags\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Tag tag in MovieParser._tags.Values)
                {
                    writer.StartRow();
                    writer.Write(tag.tagId, NpgsqlDbType.Integer);
                    writer.Write(tag.name, NpgsqlDbType.Text);
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"titles\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in MovieParser._movies.Values)
                {
                    foreach (Title title in mov.titles)
                    {
                        writer.StartRow();
                        writer.Write(title.titleId, NpgsqlDbType.Bigint);
                        writer.Write(title.title, NpgsqlDbType.Text);
                        writer.Write(mov.movieId, NpgsqlDbType.Text);
                    }
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"ActorMovie\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in MovieParser._movies.Values)
                {
                    foreach (Person actor in mov.persons)
                    {
                        writer.StartRow();
                        writer.Write(actor.personId, NpgsqlDbType.Text);
                        writer.Write(mov.movieId, NpgsqlDbType.Text);
                    }
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"MovieTag\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in MovieParser._movies.Values)
                {
                    foreach (Tag tag in mov.tags)
                    {
                        writer.StartRow();                        
                        writer.Write(mov.movieId, NpgsqlDbType.Text);
                        writer.Write(tag.tagId, NpgsqlDbType.Integer);
                    }
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"MovieMovie\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in MovieParser._movies.Values)
                {
                    foreach (Movie topmov in mov.top10)
                    {
                        writer.StartRow();                        
                        writer.Write(mov.movieId, NpgsqlDbType.Text);
                        writer.Write(topmov.movieId, NpgsqlDbType.Text);
                    }
                }
                writer.Complete();
            }

            connection.Close();
        }
    }
}
