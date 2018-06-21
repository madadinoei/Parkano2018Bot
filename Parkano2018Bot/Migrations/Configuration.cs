using System.Collections.Generic;
using Parkano2018Bot.Enums;
using Parkano2018Bot.Models;

namespace Parkano2018Bot.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Parkano2018Bot.Models.ApplicationContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationContext applicationContext)
        {

            var games = new List<Game>
            {
                new Game()
                {
                    Id = Guid.NewGuid(),
                    HomeTeam = Team.Iran.ToString(),
                    AwayTeam = Team.Spain.ToString(),
                    MatchDateTime = new DateTime(2018, 06, 20, 19, 00, 00, 00, 00)
                }
                ,new Game()
                {
                    Id = Guid.NewGuid(),
                    HomeTeam = Team.Australia.ToString(),
                    AwayTeam = Team.Peru.ToString(),
                    MatchDateTime = new DateTime(2018, 06, 21, 16, 00, 00, 00, 00)
                }
                ,new Game()
                {
                    Id = Guid.NewGuid(),
                    HomeTeam = Team.Denmark.ToString(),
                    AwayTeam = Team.France.ToString(),
                    MatchDateTime = new DateTime(2018, 06, 21, 19, 00, 00, 00, 00)
                }
            };
            applicationContext.Games.AddRange(games);

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
