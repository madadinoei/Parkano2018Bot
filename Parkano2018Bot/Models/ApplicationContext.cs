using System.Data.Entity;

namespace Parkano2018Bot.Models
{
    public class ApplicationContext :DbContext
    {
        public ApplicationContext() : base("name=SqlConnectionString")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationContext>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
    }
}