namespace Parkano2018Bot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Polls", "User_UserId", "dbo.Users");
            DropIndex("dbo.Polls", new[] { "User_UserId" });
            RenameColumn(table: "dbo.Polls", name: "User_UserId", newName: "User_Id");
            DropPrimaryKey("dbo.Users");
            AddColumn("dbo.Users", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Users", "PhoneNumber", c => c.String());
            AlterColumn("dbo.Polls", "User_Id", c => c.Int());
            AddPrimaryKey("dbo.Users", "Id");
            CreateIndex("dbo.Polls", "User_Id");
            AddForeignKey("dbo.Polls", "User_Id", "dbo.Users", "Id");
            DropColumn("dbo.Users", "UserId");
            DropColumn("dbo.Users", "Mobile");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Mobile", c => c.String());
            AddColumn("dbo.Users", "UserId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.Polls", "User_Id", "dbo.Users");
            DropIndex("dbo.Polls", new[] { "User_Id" });
            DropPrimaryKey("dbo.Users");
            AlterColumn("dbo.Polls", "User_Id", c => c.Guid());
            DropColumn("dbo.Users", "PhoneNumber");
            DropColumn("dbo.Users", "Id");
            AddPrimaryKey("dbo.Users", "UserId");
            RenameColumn(table: "dbo.Polls", name: "User_Id", newName: "User_UserId");
            CreateIndex("dbo.Polls", "User_UserId");
            AddForeignKey("dbo.Polls", "User_UserId", "dbo.Users", "UserId");
        }
    }
}
