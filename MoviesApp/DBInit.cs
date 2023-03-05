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
            public DbSet<Person> Persons => Set<Person>();
            public DbSet<Tag> Tags => Set<Tag>();
            public DbSet<Category> Categories => Set<Category>();
            public DbSet<Title> Titles => Set<Title>();

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
        public static void Init(MovieParser movparser)
        {
            using (var context = new AppContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            NpgsqlConnectionStringBuilder? connectionString = new NpgsqlConnectionStringBuilder()
            {
                Host = "localhost",
                Port = 5432,
                Database = "myFilmDb",
                Username = "postgres",
                Password = "12345",
                IncludeErrorDetail = true
            };
            NpgsqlConnection connection = new NpgsqlConnection(connectionString.ConnectionString);

            int _cid = 1;
            connection.Open();
            using (var writer = connection.BeginBinaryImport("COPY \"Movies\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in movparser._movieIdMovie.Values) 
                {
                    writer.StartRow();
                    writer.Write(mov.imdbId, NpgsqlDbType.Varchar);                    
                    writer.Write(mov.rating, NpgsqlDbType.Double);
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"Persons\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Person pers in movparser._personMovies.Keys)
                {
                    writer.StartRow();
                    writer.Write(pers.personId, NpgsqlDbType.Varchar);
                    writer.Write(pers.name, NpgsqlDbType.Text);
                    //writer.Write(pers.TypeToString(), NpgsqlDbType.Text);
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"Tags\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Tag tag in movparser._tagMovies.Keys)
                {
                    writer.StartRow();
                    writer.Write(tag.tagId, NpgsqlDbType.Integer);
                    writer.Write(tag.name, NpgsqlDbType.Text);
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"Titles\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in movparser._movieIdMovie.Values)
                {
                    foreach (Title title in mov.titles)
                    {
                        writer.StartRow();
                        writer.Write(title.titleId, NpgsqlDbType.Integer);
                        writer.Write(title.title, NpgsqlDbType.Text);
                        writer.Write(mov.imdbId, NpgsqlDbType.Varchar);
                    }
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"Categories\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in movparser._movieIdMovie.Values)
                {
                    if (mov.categories.Count == 0)
                        continue;

                    foreach (Category cat in mov.categories)
                    {
                        cat.categoryId = _cid;
                        writer.StartRow();
                        writer.Write(cat.categoryId, NpgsqlDbType.Integer);
                        writer.Write(cat.movie.imdbId, NpgsqlDbType.Varchar);
                        writer.Write(cat.person.personId, NpgsqlDbType.Varchar);                        
                        writer.Write(cat.category, NpgsqlDbType.Text);

                        _cid++;
                    }
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"MovieTag\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in movparser._movieIdMovie.Values)
                {
                    foreach (Tag tag in mov.tags)
                    {
                        writer.StartRow();                        
                        writer.Write(mov.imdbId, NpgsqlDbType.Varchar);
                        writer.Write(tag.tagId, NpgsqlDbType.Integer);
                    }
                }
                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("COPY \"MovieMovie\" FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Movie mov in movparser._movieIdMovie.Values)
                {
                    foreach (Movie topmov in mov.top10)
                    {
                        writer.StartRow();                        
                        writer.Write(mov.imdbId, NpgsqlDbType.Varchar);
                        writer.Write(topmov.imdbId, NpgsqlDbType.Varchar);
                    }
                }
                writer.Complete();
            }

            connection.Close();
            
        }
    }
}
