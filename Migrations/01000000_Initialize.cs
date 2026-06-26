using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using StudioElf.Module.CRM.MeetingNotes.Migrations.EntityBuilders;
using StudioElf.Module.CRM.MeetingNotes.Repository;

namespace StudioElf.Module.CRM.MeetingNotes.Migrations;

[DbContext(typeof(MeetingNotesContext))]
[Migration("StudioElf.Module.CRM.MeetingNotes.01.00.00.00")]
public class InitializeMeetingNotes : MultiDatabaseMigration
{
    public InitializeMeetingNotes(IDatabase database) : base(database)
    {
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var meetingNoteBuilder = new MeetingNoteEntityBuilder(migrationBuilder, ActiveDatabase);
        meetingNoteBuilder.Create();
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var meetingNoteBuilder = new MeetingNoteEntityBuilder(migrationBuilder, ActiveDatabase);
        meetingNoteBuilder.Drop();
    }
}
