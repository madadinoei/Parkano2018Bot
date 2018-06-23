namespace Parkano2018Bot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migrate2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Polls", "User_UserId", "dbo.Users");
            DropIndex("dbo.Polls", new[] { "User_UserId" });
            DropPrimaryKey("dbo.Users");
            AddColumn("dbo.Users", "PhoneNumber", c => c.String());
            AlterColumn("dbo.Users", "UserId", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Polls", "User_UserId", c => c.Int());
            AddPrimaryKey("dbo.Users", "UserId");
            CreateIndex("dbo.Polls", "User_UserId");
            AddForeignKey("dbo.Polls", "User_UserId", "dbo.Users", "UserId");
            DropColumn("dbo.Users", "Mobile");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Mobile", c => c.String());
            DropForeignKey("dbo.Polls", "User_UserId", "dbo.Users");
            DropIndex("dbo.Polls", new[] { "User_UserId" });
            DropPrimaryKey("dbo.Users");
            AlterColumn("dbo.Polls", "User_UserId", c => c.Guid());
            AlterColumn("dbo.Users", "UserId", c => c.Guid(nullable: false));
            DropColumn("dbo.Users", "PhoneNumber");
            AddPrimaryKey("dbo.Users", "UserId");
            CreateIndex("dbo.Polls", "User_UserId");
            AddForeignKey("dbo.Polls", "User_UserId", "dbo.Users", "UserId");
        }
    }
}
