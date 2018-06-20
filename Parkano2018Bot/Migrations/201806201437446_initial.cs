namespace Parkano2018Bot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        HomeTeam = c.String(),
                        AwayTeam = c.String(),
                        MatchDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Mobile = c.String(),
                        Score = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Polls",
                c => new
                    {
                        GameId = c.Guid(nullable: false),
                        HomeGoal = c.Int(nullable: false),
                        AwayGoal = c.Int(nullable: false),
                        CanEdit = c.Boolean(nullable: false),
                        User_UserId = c.Guid(),
                    })
                .PrimaryKey(t => t.GameId)
                .ForeignKey("dbo.Users", t => t.User_UserId)
                .Index(t => t.User_UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Polls", "User_UserId", "dbo.Users");
            DropIndex("dbo.Polls", new[] { "User_UserId" });
            DropTable("dbo.Polls");
            DropTable("dbo.Users");
            DropTable("dbo.Games");
        }
    }
}
