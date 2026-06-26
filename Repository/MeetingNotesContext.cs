using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Repository.Databases.Interfaces;
using StudioElf.Module.CRM.MeetingNotes.Models;

namespace StudioElf.Module.CRM.MeetingNotes.Repository;

public class MeetingNotesContext : DBContextBase, ITransientService, IMultiDatabase
{
    public MeetingNotesContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies) { }

    public DbSet<MeetingNote> MeetingNotes => Set<MeetingNote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<MeetingNote>(entity =>
        {
            entity.ToTable(ActiveDatabase.RewriteName("StudioElfCRMExtnMeetingNote"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Summary).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ActionItems).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Attendees).HasMaxLength(2000);
            entity.HasIndex(e => new { e.ContactId, e.ModuleId });
            entity.HasIndex(e => e.MeetingDate);
        });
    }
}