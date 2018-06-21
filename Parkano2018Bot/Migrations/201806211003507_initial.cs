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
                        Id = c.Guid(nullable: false),
                        TelegramUserId = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        PhoneNumber = c.String(),
                        Score = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Polls",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        HomeGoal = c.Int(nullable: false),
                        AwayGoal = c.Int(nullable: false),
                        GameId = c.Guid(nullable: false),
                        CanEdit = c.Boolean(nullable: false),
                        User_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Polls", "User_Id", "dbo.Users");
            DropIndex("dbo.Polls", new[] { "User_Id" });
            DropTable("dbo.Polls");
            DropTable("dbo.Users");
            DropTable("dbo.Games");
        }
    }
}
