# Meeting Notes — CRM Extension Specification

## Overview

A CRM extension that adds a **Meeting Notes** self-contained tab with contact selector. Users can record meeting date, duration, attendees, summary, and action items linked to CRM contacts. Meetings appear in the extension's unified list with search and sort.

**Extension ID:** `MeetingNotes`  
**Display Name:** Meeting Notes  
**Icon:** `bi bi-journal-text`  
**Version:** 1.0.0  

---

## Architecture

```
MeetingNotesExtension (ICrmExtension)
├── Shell Tab — self-contained with contact selector, search, sort
├── Dashboard Widget — recent meetings card on CRM dashboard
├── Email Template — meeting summary default template
├── Own DbContext + Migration — StudioElfCRMExtnMeetingNote table
└── GetTimelineItems — contributed to contact/deal/company timeline
```

### Generated project structure

```
StudioElf.Module.CRM.MeetingNotes/
├── Client/
│   ├── MeetingNotesShell.razor             # Self-contained shell tab
│   └── RecentMeetingsWidget.razor          # Dashboard card
├── Extensions/
│   └── MeetingNotesExtension.cs            # ICrmExtension implementation
├── Manager/
│   └── MeetingNotesManager.cs              # MigratableModuleBase + IInstallable
├── Migrations/
│   ├── 01000000_Initialize.cs              # MultiDatabaseMigration
│   └── EntityBuilders/
│       └── MeetingNoteEntityBuilder.cs     # Entity builder pattern
├── Models/
│   ├── MeetingNotesModuleInfo.cs           # Static metadata constants
│   └── MeetingNotesContracts.cs            # Entity + DTOs
├── Repository/
│   └── MeetingNotesContext.cs              # DBContextBase + IMultiDatabase
├── Services/
│   ├── IMeetingNotesService.cs             # Service interface
│   └── MeetingNotesService.cs              # IDbContextFactory implementation
├── Startup/
│   └── ServerStartup.cs                    # IServerStartup + DI
├── Package/
│   ├── debug.cmd                           # Debug build + DLL copy
│   ├── release.cmd                         # Release + NuGet pack (auto-detects nuspec)
│   ├── StudioElf.Module.CRM.MeetingNotes.nuspec
│   └── icon.png                            # Embedded CRM icon
├── ModuleInfo.cs                           # Oqtane IModule registration
├── StudioElf.Module.CRM.MeetingNotes.slnx  # Solution with Oqtane.Server for debugging
└── StudioElf.Module.CRM.MeetingNotes.csproj
```

---

## Data Model

### MeetingNote : ModelBase

```csharp
public class MeetingNote : ModelBase  // CreatedBy, CreatedOn, ModifiedBy, ModifiedOn inherited
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int ContactId { get; set; }
    public int? CompanyId { get; set; }
    public int? DealId { get; set; }

    public string Title { get; set; }
    public string Summary { get; set; }
    public DateTime MeetingDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Location { get; set; }
    public string ActionItems { get; set; }   // JSON array
    public string Attendees { get; set; }     // Comma-separated names
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

### 1. Shell Tab (self-contained)

Rendered as an extension tab in the CRM tab bar. Self-contained — does NOT inject into contact detail view.

**Location:** `Client/MeetingNotesShell.razor`  
**Features:**
- Contact selector dropdown — loads all CRM contacts
- Search bar — filter by title, summary, location, creator
- Sort — Date, Title, Contact with ascending/descending toggle
- All meetings list with inline edit/delete
- Add meeting modal with contact picker
- Fields: contact, title, date, duration, location, attendees, summary
- Send Summary button per meeting (email template integration)

### 2. Dashboard Widget

Recent meetings card on CRM dashboard.

**Location:** `Client/RecentMeetingsWidget.razor`  
**Features:**
- Shows last 5 recent meetings across all contacts
- Rendered via `DynamicComponent` on CRM dashboard

### 3. Email Template

Default template seeded on install via `GetEmailTemplates()`:

```
Template Name: Meeting Summary
Subject: Meeting Summary: {{MeetingTitle}}
Body: A meeting was held on {{MeetingDate}} with {{AttendeeCount}} attendees.
{{Summary}}
Action Items:
{{ActionItems}}
```

Placeholder tokens: `{{MeetingTitle}}`, `{{MeetingDate}}`, `{{AttendeeCount}}`, `{{Summary}}`, `{{ActionItems}}`

### 4. Timeline Contribution

```csharp
public List<TimelineItem> GetTimelineItems(...) => null;  // returns null when unused
```

---

## API Surface

```csharp
public interface IMeetingNotesService
{
    Task<List<MeetingNoteDto>> GetAllAsync(int moduleId);
    Task<List<MeetingNoteDto>> GetByContactAsync(int contactId, int moduleId);
    Task<MeetingNoteDto> CreateAsync(CreateMeetingNoteDto dto, int moduleId, string createdBy);
    Task<MeetingNoteDto> UpdateAsync(int id, CreateMeetingNoteDto dto, int moduleId, string createdBy);
    Task DeleteAsync(int id, int moduleId);
    Task<List<MeetingNoteDto>> GetRecentAsync(int moduleId, string createdBy, int count = 5);
}
```

---

## CRITICAL — Exact Implementation Reference

The following code patterns are **mandatory**. AI codegen must produce these exact signatures. Deviation causes runtime failures.

### ⚠️ ModuleInfo.cs — Oqtane Module Registration (MUST match this exactly)

```csharp
using Oqtane.Models;
using Oqtane.Modules;
using StudioElf.Module.CRM;
using StudioElf.Module.CRM.MeetingNotes.Models;

namespace StudioElf.Module.CRM.MeetingNotes;

public class ModuleInfo : IModule
{
    public ModuleDefinition ModuleDefinition => new ModuleDefinition
    {
        Name = MeetingNotesModuleInfo.DisplayName,
        Description = MeetingNotesModuleInfo.Description,
        Categories = "Headless",  // prevents showing in Oqtane module picker
        Version = MeetingNotesModuleInfo.Version,
        ReleaseVersions = VersionInfo.Version,  // migration triggers on version bump
        ServerManagerType = "StudioElf.Module.CRM.MeetingNotes.Manager.MeetingNotesManager, StudioElf.Module.CRM.MeetingNotes.Oqtane",
        Dependencies = "StudioElf.Module.CRM.Shared.Oqtane",  // CRM shared assembly only
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
using StudioElf.Module.CRM.MeetingNotes.Services;

namespace StudioElf.Module.CRM.MeetingNotes.Startup;

public class ServerStartup : IServerStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICrmExtension>(sp => new MeetingNotesExtension());
        services.AddDbContextFactory<MeetingNotesContext>(opt => { }, ServiceLifetime.Transient);
        services.AddScoped<IMeetingNotesService, MeetingNotesService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { }
    public void ConfigureMvc(IMvcBuilder mvcBuilder) { }
}
```

### ⚠️ Models/MeetingNotesModuleInfo.cs — Static metadata constants

```csharp
namespace StudioElf.Module.CRM.MeetingNotes.Models;

public static class MeetingNotesModuleInfo
{
    public const string ExtensionId = "MeetingNotes";
    public const string DisplayName = "Meeting Notes";
    public const string Description = "Contacts Meeting Notes";
    public const string Version = "0.1.0";
    public const string IconClass = "bi bi-journal-text";
}
```

### ⚠️ MeetingNotesExtension.cs — ICrmExtension (MUST match this exactly)

```csharp
// No constructor parameters. No fields. No DI.
public class MeetingNotesExtension : ICrmExtension
{
    public string ExtensionId => MeetingNotesModuleInfo.ExtensionId;
    public string DisplayName => MeetingNotesModuleInfo.DisplayName;
    public string Description => MeetingNotesModuleInfo.Description;
    public string Version => MeetingNotesModuleInfo.Version;
    public string IconClass => MeetingNotesModuleInfo.IconClass;

    public Type GetShellComponentType() => typeof(MeetingNotesShell);

    // Return empty lists, never null
    public List<CrmNavItem> GetNavItems() => new();
    public List<CrmDashboardWidget> GetDashboardWidgets() => new()
    {
        new("recent-meetings", "Recent Meetings", typeof(RecentMeetingsWidget), 10)
    };
    public List<CrmContactTab> GetContactTabs() => new();  // self-contained tab — no contact injection
    public List<CrmEmailTemplate> GetEmailTemplates() => new()
    {
        new("Meeting Summary", "Meeting Summary: {{MeetingTitle}}",
            "A meeting was held on {{MeetingDate}} with {{AttendeeCount}} attendees.\n\n{{Summary}}\n\nAction Items:\n{{ActionItems}}")
    };
    public List<TimelineItem> GetTimelineItems(...) => null;
}
```

### ⚠️ MeetingNotesService.cs — IDbContextFactory injection

```csharp
public class MeetingNotesService : IMeetingNotesService
{
    private readonly IDbContextFactory<MeetingNotesContext> _factory;

    public MeetingNotesService(IDbContextFactory<MeetingNotesContext> factory)
    {
        _factory = factory;  // create + dispose per operation
    }

    public async Task<List<MeetingNoteDto>> GetAllAsync(int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.MeetingNotes
            .Where(m => m.ModuleId == moduleId)
            .OrderByDescending(m => m.MeetingDate)
            .Select(m => ToDto(m))
            .ToListAsync();
    }
    // ... remaining CRUD methods follow same pattern
}
```

### ⚠️ MeetingNotesContext.cs — DBContextBase registration

```csharp
public class MeetingNotesContext : DBContextBase, ITransientService, IMultiDatabase
{
    public MeetingNotesContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies) { }
    // OnConfiguring handled by DBContextBase — resolves tenant connection
    public DbSet<MeetingNote> MeetingNotes => Set<MeetingNote>();
}
```

---

## Implementation Steps

1. Scaffold extension named `MeetingNotes` via CRM Extension Manager
2. Update `Models/MeetingNotesContracts.cs` — add `MeetingNote : ModelBase`, `MeetingNoteDto`, `CreateMeetingNoteDto`
3. Update `Repository/MeetingNotesContext.cs` — add `DbSet<MeetingNote>` extending `DBContextBase` (see exact pattern above)
4. Create `Migrations/EntityBuilders/MeetingNoteEntityBuilder.cs` — entity builder pattern
5. Update `Migrations/01000000_Initialize.cs` — `MultiDatabaseMigration` with `[DbContext]`/`[Migration]` attributes
6. Create `Services/IMeetingNotesService.cs` — interface with `GetAllAsync`
7. Create `Services/MeetingNotesService.cs` — `IDbContextFactory<MeetingNotesContext>` injection (see exact pattern above)
8. Update `Startup/ServerStartup.cs` — DI registration (see exact pattern above)
9. Create `Client/MeetingNotesShell.razor` — self-contained shell with contact selector, search, sort
10. Create `Client/RecentMeetingsWidget.razor` — dashboard widget
11. Update `Extensions/MeetingNotesExtension.cs` — wire up dashboard widget, email templates (see exact pattern above)
12. Build (`dotnet build`) — DLL auto-copies to Oqtane Server bin via `CopyToOqtane`
13. Restart Oqtane server — extension loads via `IServerStartup` assembly scan
14. Test: CRM tab bar → Meeting Notes → select contact → add meetings

---

## Authorization

- Respect CRM module View/Edit permissions
- Users with View permission can see meetings
- Users with Edit permission can create/edit/delete
- Respect `SeedMode` (block writes in Demo/Training)

---

## Package and Deploy

```bash
cd Package
release.cmd net10.0  # auto-detects .nuspec file
```

Output: `Oqtane.Server/Packages/StudioElf.Module.CRM.MeetingNotes.0.1.0.nupkg`

Release build from Visual Studio runs `release.cmd` automatically via `PostBuildPackage` target in `.csproj`.
