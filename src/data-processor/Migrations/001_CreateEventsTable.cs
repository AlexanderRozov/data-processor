using FluentMigrator;

namespace DataProcessor.Migrations;

[Migration(1)]
public class CreateEventsTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("events").Exists())
        {
            Create.Table("events")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("value").AsInt32().NotNullable();
        }
    }

    public override void Down()
    {
        Delete.Table("events");
    }
}



