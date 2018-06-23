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
                //new Game()
                //{
                //    HomeTeam = Team.Iran.GetDescription(),
                //    AwayTeam = Team.Spain.GetDescription(),
                //    MatchDateTime = new DateTime(2018, 06, 20, 19, 00, 00, 00, 00)
                //}
                //,new Game()
                //{
                //    HomeTeam = Team.Australia.GetDescription(),
                //    AwayTeam = Team.Peru.GetDescription(),
                //    MatchDateTime = new DateTime(2018, 06, 21, 16, 00, 00, 00, 00)
                //}
                //,new Game()
                //{
                //    HomeTeam = Team.Denmark.GetDescription(),
                //    AwayTeam = Team.France.GetDescription(),
                //    MatchDateTime = new DateTime(2018, 06, 23, 19, 00, 00, 00, 00)
                //},
                new Game()
                {
                    HomeTeam = Team.Belgium.GetDescription(),
                    AwayTeam = Team.Tunisia.GetDescription(),
                    MatchDateTime=new DateTime(2018, 06, 23, 16, 30, 00, 00, 00)
                },
                new Game()
                {
                    HomeTeam = Team.KoreaRepublic.GetDescription(),
                    AwayTeam = Team.Mexico.GetDescription(),
                    MatchDateTime=new DateTime(2018, 06, 23, 19, 30, 00, 00, 00)
                },  new Game()
                {
                    HomeTeam = Team.Germany.GetDescription(),
                    AwayTeam = Team.Sweden.GetDescription(),
                    MatchDateTime=new DateTime(2018, 06, 23, 22, 30, 00, 00, 00)
                },  new Game()
                {
                    HomeTeam = Team.England.GetDescription(),
                    AwayTeam = Team.Panama.GetDescription(),
                    MatchDateTime=new DateTime(2018, 06, 23, 16, 30, 00, 00, 00)
                },  new Game()
                {
                    HomeTeam = Team.Japan.GetDescription(),
                    AwayTeam = Team.Senegal.GetDescription(),
                    MatchDateTime=new DateTime(2018, 06, 23, 19, 30, 00, 00, 00)
                },  new Game()
                {
                    HomeTeam = Team.Poland.GetDescription(),
                    AwayTeam = Team.Colombia.GetDescription(),
                    MatchDateTime=new DateTime(2018, 06, 23, 22, 30, 00, 00, 00)
                },
            };
            applicationContext.Database.ExecuteSqlCommand("delete from Games");
            applicationContext.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('[Games]', RESEED, 0)");

            applicationContext.Games.AddRange(games);

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
