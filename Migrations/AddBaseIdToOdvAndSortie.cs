using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddBaseIdToOdvAndSortie : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add BaseId columns
        migrationBuilder.AddColumn<int>(
            name: "BaseId",
            table: "Odvs",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "BaseId",
            table: "Sorties",
            type: "int",
            nullable: true);

        // Simple nonclustered indexes created by EF will also be generated from modelBuilder.HasIndex,
        // but create covering indexes explicitly for common queries that select specific columns:
        // Example: queries for list of odvs by base+date that also display SquadronId and OdvStatus
        migrationBuilder.Sql(@"
            CREATE NONCLUSTERED INDEX IX_Odvs_Base_OdvDate_Covering
            ON dbo.Odvs (BaseId, OdvDate)
            INCLUDE (SquadronId, OdvStatus, AcMainGroupId, CreatedAtUtc);
        ");

        // Example: sorties by Base and StartTime with include of FuelQuantity, AircraftId
        migrationBuilder.Sql(@"
            CREATE NONCLUSTERED INDEX IX_Sorties_Base_StartTime_Covering
            ON dbo.Sorties (BaseId, StartTime)
            INCLUDE (AircraftId, FuelQuantity, OdvId, IsCompleted);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Sorties_Base_StartTime_Covering ON dbo.Sorties;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Odvs_Base_OdvDate_Covering ON dbo.Odvs;");

        migrationBuilder.DropColumn(name: "BaseId", table: "Sorties");
        migrationBuilder.DropColumn(name: "BaseId", table: "Odvs");
    }
}
