using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class SyncWorkSectionAcMainGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — the DB was already brought to this
            // state by a hand-run SQL script (WorkSection_AcMainGroup_
            // Migration.sql) that merged duplicate F16C/F16D and F5E/F5F
            // rows before renaming the column. This migration exists only
            // to sync EF's model snapshot (FRAContextModelSnapshot.cs)
            // with the C# model — running the scaffolded rename here would
            // fail, since AcTypeId/its FK/its index no longer exist.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — see Up(). A true rollback of the
            // AcMainGroup merge would need to reverse the duplicate-row
            // split, which isn't something this migration can safely
            // automate; restore from backup if you need to revert.
        }
    }
}