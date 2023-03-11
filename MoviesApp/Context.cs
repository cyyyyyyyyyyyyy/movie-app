using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp
{
	public class AppContext : DbContext
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
}
