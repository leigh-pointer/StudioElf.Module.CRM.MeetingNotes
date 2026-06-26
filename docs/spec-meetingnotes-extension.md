# Meeting Notes — CRM Extension Specification

## Overview

A CRM extension that adds a **Meeting Notes** tab to contacts. Users can record meeting date, duration, attendees, summary, and action items. Notes appear in the CRM unified timeline.

**Extension ID:** `MeetingNotes`  
**Display Name:** Meeting Notes  
**Icon:** `bi bi-journal-text`  
**Version:** 1.0.0  

---

## Architecture

```
MeetingNotesExtension (ICrmExtension)
├── Contact Tab — rendered in contact detail view
├── Dashboard Widget — recent meetings widget on CRM dashboard
├── Email Template — meeting summary default template
├── Own DbContext + Migration — MeetingNotes database table
└── Timeline Items — contributed to contact/deal/company timeline
```

### Generated project structure

```
StudioElf.Module.CRM.MeetingNotes/
├── Client/
│   └── MeetingNotesShell.razor            # Main shell component (contact tab)
├── Extensions/
│   └── MeetingNotesExtension.cs            # ICrmExtension implementation
├── Manager/
│   └── MeetingNotesManager.cs              # IInstallable (install/uninstall)
├── Migrations/
│   └── 01000000_Initialize.cs              # EF Core migration
├── Models/
│   ├── MeetingNotesModuleInfo.cs           # Static metadata constants
│   └── MeetingNotesContracts.cs            # DTOs and settings
├── Repository/
│   └── MeetingNotesContext.cs              # EF Core DbContext
├── Startup/
│   └── ServerStartup.cs                    # DI registration (migrations via IInstallable)
├── Package/
│   ├── debug.cmd
│   ├── release.cmd
│   ├── MeetingNotes.nuspec
│   └── icon.png
├── ModuleInfo.cs                           # Oqtane IModule registration
├── StudioElf.Module.CRM.MeetingNotes.slnx  # Solution with Oqtane.Server
└── StudioElf.Module.CRM.MeetingNotes.csproj
```

---

## Data Model

### MeetingNote

```csharp
public class MeetingNote
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int ContactId { get; set; }          // FK to CRM contact
    public int? CompanyId { get; set; }          // Optional FK to CRM company
    public int? DealId { get; set; }             // Optional FK to CRM deal

    public string Title { get; set; }            // Meeting title
    public string Summary { get; set; }          // Free-text summary (markdown supported)
    public string ActionItems { get; set; }      // JSON array of action items
    public string Attendees { get; set; }        // JSON array of attendee names/emails
    public DateTime MeetingDate { get; set; }    // When the meeting occurred
    public int DurationMinutes { get; set; }     // Meeting length
    public string Location { get; set; }         // Physical or virtual meeting location

    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? ModifiedByUserId { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
```

### ActionItem JSON structure

```json
[
    { "text": "Follow up on pricing proposal", "assignedTo": "John", "dueDate": "2026-06-20", "completed": false }
]
```

---

## UI Surfaces

### 1. Contact Tab

Rendered in contact detail view. Receives `ContactId` via cascading parameter.

**Location:** `Client/MeetingNotesShell.razor`  
**Tab label:** "Meeting Notes"  
**Order:** 50  

Features:
- List of meetings for this contact (newest first)
- "Add Meeting" button opens inline form
- Inline edit/delete on existing meetings
- Fields: title, date, duration, location, summary (textarea), attendees (tag-style input), action items (checklist)
- Action items show checkbox — checking marks as complete
- All text localizable via resx

### 2. Dashboard Widget

Recent meetings widget on CRM dashboard.

**Component:** same as contact tab, filtered to current user's recent meetings across all contacts  
**Order:** 30  

Features:
- Shows last 5 meetings for current user
- Click navigates to associated contact

### 3. Timeline Contribution

When viewed in CRM timeline for a contact/company/deal, Meeting Notes contributes items:

```csharp
new TimelineItem
{
    ItemType = "MeetingNote",
    Title = meeting.Title,
    Description = meeting.Summary,
    Date = meeting.MeetingDate,
    Url = $"/contact/{meeting.ContactId}",
    ExtensionId = "MeetingNotes"
}
```

### 4. Email Template

Default template seeded on install:

```
Subject: Meeting Summary: {{MeetingTitle}}
Body: A meeting was held on {{MeetingDate}} with {{AttendeeCount}} attendees.
{{Summary}}
Action Items:
{{ActionItems}}
```

---

## API Surface

Server service (registered as scoped):

```csharp
public interface IMeetingNotesService
{
    Task<List<MeetingNoteDto>> GetByContactAsync(int contactId, int moduleId);
    Task<MeetingNoteDto> GetByIdAsync(int id, int moduleId);
    Task<MeetingNoteDto> CreateAsync(MeetingNoteDto dto);
    Task<MeetingNoteDto> UpdateAsync(MeetingNoteDto dto);
    Task DeleteAsync(int id, int moduleId);
    Task<List<MeetingNoteDto>> GetRecentAsync(int moduleId, int userId, int count = 5);
}
```

---

---

## CRITICAL — Exact Implementation Reference

The following code patterns are **mandatory**. AI codegen must produce these exact signatures. Deviation causes runtime failures.

### ⚠️ ModuleInfo.cs — Oqtane Module Registration (MUST match this exactly)

```csharp
// CRITICAL: Required for Oqtane to register the module and trigger ReleaseVersions migration.
// CRITICAL: Categories = "Headless" prevents showing in Oqtane module picker.
//           CRM discovers extensions via DI (ICrmExtension), not the module picker.
// CRITICAL: ServerManagerType points to Manager class for install logic.
// CRITICAL: Dependencies = CRM shared assembly (not the extension's own assembly).
using Oqtane.Models;
using Oqtane.Modules;
using StudioElf.Module.CRM.MeetingNotes.Models;

namespace StudioElf.Module.CRM.MeetingNotes;

public class ModuleInfo : IModule
{
    public ModuleDefinition ModuleDefinition => new ModuleDefinition
    {
        Name = "Meeting Notes",
        Description = "Adds meeting notes to CRM contacts.",
        Categories = "Headless",
        Version = MeetingNotesModuleInfo.Version,
        ReleaseVersions = "1.0.0",
        ServerManagerType = "StudioElf.Module.CRM.MeetingNotes.Manager.MeetingNotesManager, StudioElf.Module.CRM.MeetingNotes.Oqtane",
        Dependencies = "StudioElf.Module.CRM.Shared.Oqtane",
        PackageName = "StudioElf.Module.CRM.MeetingNotes"
    };
}
```

### ⚠️ ServerStartup.cs — DI Registration (MUST match this exactly)

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using StudioElf.Module.CRM.Extensions;
using StudioElf.Module.CRM.Services;
using StudioElf.Module.CRM.MeetingNotes.Repository;

namespace StudioElf.Module.CRM.MeetingNotes.Startup;

public class ServerStartup : IServerStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ICrmExtension must be Singleton. Factory takes NO constructor params.
        services.AddSingleton<ICrmExtension>(sp => new MeetingNotesExtension());
        // DbContext factory. Empty options = Oqtane resolves tenant connection automatically.
        // For external data sources, replace with: opt => opt.UseSqlServer(connectionString)
        services.AddDbContextFactory<MeetingNotesContext>(opt => { }, ServiceLifetime.Transient);
        services.AddScoped<IMeetingNotesService, MeetingNotesService>();
    }

    // CRITICAL: Configure and ConfigureMvc must exist for IServerStartup contract.
    // Migrations are handled by Oqtane module installer via ReleaseVersions in ModuleInfo.cs.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { }
    public void ConfigureMvc(IMvcBuilder mvcBuilder) { }
}
```

### ⚠️ Models/MeetingNotesModuleInfo.cs — Static metadata constants (single source of truth)

```csharp
// CRITICAL: Single source of truth for ALL extension metadata.
// CRITICAL: ICrmExtension, ModuleInfo, and any other code reads from here.
//           No hardcoded strings scattered across files.
// CRITICAL: Class naming = {ExtensionName}ModuleInfo (matches scaffolder output).
namespace StudioElf.Module.CRM.MeetingNotes.Models;

public static class MeetingNotesModuleInfo
{
    public const string ExtensionId = "MeetingNotes";
    public const string DisplayName = "Meeting Notes";
    public const string Description = "Adds meeting notes to CRM contacts.";
    public const string Version = "1.0.0";
    public const string IconClass = "bi bi-journal-text";
    public const int ContactTabOrder = 50;
    public const int DashboardWidgetOrder = 30;
}
```

### ⚠️ MeetingNotesExtension.cs — ICrmExtension (MUST match this exactly)

```csharp
// CRITICAL: No constructor parameters. No fields. No DI.
//           The factory in ServerStartup is 'sp => new MeetingNotesExtension()'.
//           All metadata comes from MeetingNotesModuleInfo constants.
// CRITICAL: GetShellComponentType() returns typeof(MeetingNotesShell) — a Blazor component.
// CRITICAL: Return empty lists, not null, for optional surfaces.
public class MeetingNotesExtension : ICrmExtension
{
    public string ExtensionId => MeetingNotesModuleInfo.ExtensionId;
    public string DisplayName => MeetingNotesModuleInfo.DisplayName;
    public string Description => MeetingNotesModuleInfo.Description;
    public string Version => MeetingNotesModuleInfo.Version;
    public string IconClass => MeetingNotesModuleInfo.IconClass;

    public Type GetShellComponentType() => typeof(MeetingNotesShell);

    public List<CrmNavItem> GetNavItems() => new();
    public List<CrmDashboardWidget> GetDashboardWidgets() => new();
    public List<CrmContactTab> GetContactTabs() => new()
    {
        new("meeting-notes", MeetingNotesModuleInfo.DisplayName, typeof(MeetingNotesShell), MeetingNotesModuleInfo.ContactTabOrder)
    };
    public List<CrmEmailTemplate> GetEmailTemplates() => new();
    public List<TimelineItem> GetTimelineItems(string entityName, int entityId, int moduleId, TimelineFilter filter) => null;
}
```

### ⚠️ MeetingNotesService.cs — Constructor injection (MUST use IDbContextFactory)

```csharp
// CRITICAL: Use IDbContextFactory<T>, never inject T directly.
//           Oqtane uses transient DbContext factories — create + dispose per operation.
// CRITICAL: Each method creates its own context via _factory.CreateDbContext().
// CRITICAL: Check module permissions before write operations.
public class MeetingNotesService : IMeetingNotesService
{
    private readonly IDbContextFactory<MeetingNotesContext> _factory;

    public MeetingNotesService(IDbContextFactory<MeetingNotesContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<MeetingNoteDto>> GetByContactAsync(int contactId, int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.MeetingNotes
            .Where(m => m.ContactId == contactId && m.ModuleId == moduleId)
            .OrderByDescending(m => m.MeetingDate)
            .Select(m => ToDto(m))
            .ToListAsync();
    }

    // ... remaining CRUD methods follow same pattern
}
```

### ⚠️ MeetingNotesContext.cs — DbContext registration

```csharp
// CRITICAL: No OnConfiguring override. Oqtane resolves connection via AddDbContextFactory.
// CRITICAL: No UseSqlServer/UseSqlite calls anywhere.
public class MeetingNotesContext : DbContext
{
    public MeetingNotesContext(DbContextOptions<MeetingNotesContext> options)
        : base(options) { }

    public DbSet<MeetingNote> MeetingNotes => Set<MeetingNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeetingNote>(entity =>
        {
            entity.ToTable("MeetingNote");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Summary).HasColumnType("nvarchar(max)");
            entity.HasIndex(e => new { e.ContactId, e.ModuleId });
        });
    }
}
```

---

## Implementation Steps

1. Scaffold extension named `MeetingNotes` via CRM Extension Manager
2. Update `Models/MeetingNotesContracts.cs` — add `MeetingNoteDto`, `CreateMeetingNoteDto`, `UpdateMeetingNoteDto`
3. Update `Repository/MeetingNotesContext.cs` — add `DbSet<MeetingNote>` and configure entity (see exact pattern above)
4. Update `Migrations/01000000_Initialize.cs` — create MeetingNotes table with proper indexes
5. Create `Services/IMeetingNotesService.cs` — interface
6. Create `Services/MeetingNotesService.cs` — implementation with `IDbContextFactory<MeetingNotesContext>` (see exact pattern above)
7. Create `Controllers/MeetingNotesController.cs` — REST endpoints
8. Update `Startup/ServerStartup.cs` — DI registration (see exact pattern above)
9. Update `Client/MeetingNotesShell.razor` — full contact tab UI
10. Update `Extensions/MeetingNotesExtension.cs` — wire up contact tab, dashboard widget, timeline items, email template (see exact pattern above)
11. Build (`dotnet build`)
12. Restart Oqtane server (extension DLL must be in Oqtane.Server/bin for `IServerStartup` discovery)
13. Test with a contact

---

## Authorization

- Respect CRM module View/Edit permissions
- Users with View permission can see meetings
- Users with Edit permission can create/edit/delete
- Respect `SeedMode` (block writes in Demo/Training)

---

## Localization

All UI strings in `Client/Resources/` resx file. Add keys:

| Key | Default |
|-----|---------|
| MeetingNotes.Title | Meeting Notes |
| MeetingNotes.Add | Add Meeting |
| MeetingNotes.Edit | Edit Meeting |
| MeetingNotes.Delete | Delete Meeting |
| MeetingNotes.TitleField | Title |
| MeetingNotes.Date | Date |
| MeetingNotes.Duration | Duration (min) |
| MeetingNotes.Location | Location |
| MeetingNotes.Summary | Summary |
| MeetingNotes.Attendees | Attendees |
| MeetingNotes.ActionItems | Action Items |
| MeetingNotes.Save | Save |
| MeetingNotes.Cancel | Cancel |

---

## Package and Deploy

```bash
cd Package
release.cmd net10.0 StudioElf.Module.CRM.MeetingNotes
```

Output: `Oqtane.Server/Packages/StudioElf.Module.CRM.MeetingNotes.1.0.0.nupkg`
