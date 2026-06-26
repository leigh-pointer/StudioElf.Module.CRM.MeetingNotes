using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;
using StudioElf.Module.CRM.MeetingNotes.Repository;

namespace StudioElf.Module.CRM.MeetingNotes.Manager;

public class MeetingNotesManager : MigratableModuleBase, IInstallable
{
    private readonly IDBContextDependencies _DBContextDependencies;

    public MeetingNotesManager(IDBContextDependencies DBContextDependencies)
    {
        _DBContextDependencies = DBContextDependencies;
    }

    public bool Install(Tenant tenant, string version)
    {
        return Migrate(new MeetingNotesContext(_DBContextDependencies), tenant, MigrationType.Up);
    }

    public bool Uninstall(Tenant tenant)
    {
        return Migrate(new MeetingNotesContext(_DBContextDependencies), tenant, MigrationType.Down);
    }
}
