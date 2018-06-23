namespace Parkano2018Bot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class third : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Polls", "HomeGoal", c => c.Int());
            AlterColumn("dbo.Polls", "AwayGoal", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Polls", "AwayGoal", c => c.Int(nullable: false));
            AlterColumn("dbo.Polls", "HomeGoal", c => c.Int(nullable: false));
        }
    }
}
