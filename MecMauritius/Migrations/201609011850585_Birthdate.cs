namespace MecMauritius.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Birthdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "firstName", c => c.String());
            AddColumn("dbo.AspNetUsers", "lastName", c => c.String());
            AddColumn("dbo.AspNetUsers", "BirthDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "BirthDate");
            DropColumn("dbo.AspNetUsers", "lastName");
            DropColumn("dbo.AspNetUsers", "firstName");
        }
    }
}
