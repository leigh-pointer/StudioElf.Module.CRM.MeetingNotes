using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace StudioElf.Module.CRM.MeetingNotes.Migrations.EntityBuilders;

public class MeetingNoteEntityBuilder : AuditableBaseEntityBuilder<MeetingNoteEntityBuilder>
{
    private const string _entityTableName = "StudioElfCRMExtnMeetingNote";
    private readonly PrimaryKey<MeetingNoteEntityBuilder> _primaryKey = new("PK_StudioElfCRMExtnMeetingNote", x => x.Id);

    public MeetingNoteEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
    {
        EntityTableName = _entityTableName;
        PrimaryKey = _primaryKey;
    }

    protected override MeetingNoteEntityBuilder BuildTable(ColumnsBuilder table)
    {
        Id = AddAutoIncrementColumn(table, "Id");
        ModuleId = AddIntegerColumn(table, "ModuleId");
        ContactId = AddIntegerColumn(table, "ContactId");
        CompanyId = AddIntegerColumn(table, "CompanyId", true);
        DealId = AddIntegerColumn(table, "DealId", true);
        Title = AddStringColumn(table, "Title", 500);
        Summary = AddMaxStringColumn(table, "Summary", true);
        MeetingDate = AddDateTimeColumn(table, "MeetingDate");
        DurationMinutes = AddIntegerColumn(table, "DurationMinutes");
        Location = AddStringColumn(table, "Location", 500, true);
        ActionItems = AddMaxStringColumn(table, "ActionItems", true);
        Attendees = AddStringColumn(table, "Attendees", 2000, true);
        AddAuditableColumns(table);
        return this;
    }

    public OperationBuilder<AddColumnOperation> Id { get; set; }
    public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
    public OperationBuilder<AddColumnOperation> ContactId { get; set; }
    public OperationBuilder<AddColumnOperation> CompanyId { get; set; }
    public OperationBuilder<AddColumnOperation> DealId { get; set; }
    public OperationBuilder<AddColumnOperation> Title { get; set; }
    public OperationBuilder<AddColumnOperation> Summary { get; set; }
    public OperationBuilder<AddColumnOperation> MeetingDate { get; set; }
    public OperationBuilder<AddColumnOperation> DurationMinutes { get; set; }
    public OperationBuilder<AddColumnOperation> Location { get; set; }
    public OperationBuilder<AddColumnOperation> ActionItems { get; set; }
    public OperationBuilder<AddColumnOperation> Attendees { get; set; }
}
