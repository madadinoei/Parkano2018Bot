using System.Data.Entity;
using Parkano2018Bot.Migrations;

namespace Parkano2018Bot.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext() : base("name=SqlConnectionString")
        {
            //Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationContext>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }

        /// <param name="modelBuilder"> The builder that defines the model for the context being created. </param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationContext, Configuration>());
        }
    }
}