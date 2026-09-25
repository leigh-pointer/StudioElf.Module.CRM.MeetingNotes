# Module-Specific AI Instructions

These instructions extend the canonical Copilot entry point
(`.github/copilot-instructions.md`) and sit under the CRMX governance set.

Read, in order, before any change:

1. `..\..\docs\governance-crm\CRMX-000-index.md` (synced copy; canonical set is in the CRM core repo)
2. `..\..\..\..\DevStudio.10\StudioElf.Module.CRM\.github\module-instructions.md` (the CRM-wide record)
3. this file

All relative paths are from the root of this repository.

## CRM-wide extension lessons (inherited - these bind every extension)

### Security and messaging
- Every server endpoint needs `[Authorize(Policy = PolicyNames.ViewModule)]` for reads and
  `PolicyNames.EditModule` for writes, plus the `IsAuthorizedEntityId(EntityNames.Module, moduleId)`
  guard. HTTP file URLs must carry both `moduleId` and `authmoduleid`.
- Never render user-controlled markdown or HTML directly. Run it through a sanitizer first.
- Persist the record BEFORE sending email or firing side effects. A send failure must never lose data.
- After a save that must succeed, retry once on `DbUpdateConcurrencyException`. Resolve references by
  name or in batch; never N+1.
- Log the full exception server-side; return a generic message to the client (CRMX-005).

### Packaging and dependencies
- `debug.cmd` / `release.cmd` fail with exit code 4 "Sharing violation" while Oqtane.Server is
  running. Stop the server before packaging, restart it after.
- Third-party packages that are not part of Oqtane must be listed in the nuspec, copied by the
  csproj's copy target AND by `debug.cmd`. Satellite resource dlls (culture folders) as well.
- Framework-owned packages (EF, ASP.NET, ImageSharp) must match the framework's pin exactly.
  Module-owned packages (Markdig) must match the CRM core's pin, because this repo does not copy
  them - the running copy is whatever the CRM installed into the Oqtane bin.
- Oqtane and CRM assemblies come by `HintPath`, never `ProjectReference`.
- After any framework upgrade, re-check third-party ABI by scanning the built dlls' MemberRefs
  against the host assemblies. Optional-argument defaults are baked into call sites at compile
  time, so a 1-arg call site never matches a 2-arg method - that is the `MissingMethodException`
  class of defect (CRMX-002).

### Localization (CRMX-008)
- Extension resx sets stay in sync in ALL five files (base + `de-DE`, `es-ES`, `fr-FR`, `nl-NL`)
  with identical key names.
- The marker class for `IStringLocalizer<T>` sits at the PROJECT ROOT, never beside the resx files -
  same-folder same-base-name triggers `DependentUpon`, which silently drops the resource.
- With `GenerateAssemblyInfo=false`, an `AssemblyInfo.cs` carries `[assembly: RootNamespace("...")]`
  matching the csproj `RootNamespace`.
- Name-keyed DATA (status names, option rows) is never localized. Only UI text goes through the localizer.
- `[Inject]` in hand-written `.cs` base classes needs `using Microsoft.AspNetCore.Components;`.
  Injected `Loc` and other instance members cannot be referenced from `static` methods.

### CSS (CRMX-009)
- Never style CRM structural classes (`.crm-tabpanel`, `.crm-shell .crm-*`). The Helpdesk extension
  did, and its `overflow`/`contain: inline-size` rule silently clipped absolutely positioned
  popovers in the CRM shell.
- Any `overflow` value other than `visible` on one axis forces the other axis off `visible`.
  Scope every rule to this extension's own prefixed classes.

### Controller routing
- `ServiceBase.CreateApiUrl(name)` produces `/api/{name}`, which matches `api/[controller]` only when
  `name` equals the controller class name minus `Controller`. A wrong name is a silent 404 on every call.

### Diagnostics
- A read-only page load may perform no work and raise no error. Oqtane turns Error-level logs into
  host alert emails, so gate on permission silently and keep the Security log on explicit,
  user-initiated entry points only (CRMX-010).

### Custom fields (CRMX-003)
- Declare fields in code; the core imports them at startup and purges them on uninstall. No import hook.
- A changed declaration does nothing until the extension is rebuilt, redeployed AND the server
  restarted. A stale dll leaves the live table on the old declaration with no error logged.
- SDK record constructors are never removed: extension dlls are shipped artifacts and outlive the SDK
  they were built against.

### Dates (CRMX-014)
- Store UTC. `DateTime.UtcNow`, never `DateTime.Now` / `DateTime.Today`. Never convert before persisting.
- Render through the inherited helpers on `CrmBase`: `FormatLocalDate(...)`, `ToLocalTime(...)` for a
  `DateTime` you must compare or bind, `ToUtcTime(...)` on the way back to storage. Never
  `DateTime.ToLocalTime()` - it uses the process zone, ignores the user's and site's timezone
  settings, and converts nothing at all on `DateTimeKind.Unspecified` values.
- Never re-declare these helpers in an extension base class. A nullable overload shadows the inherited
  member and the extension silently keeps the old behaviour.
- House formats only: `dd-MMM-yyyy HH:mm:ss` (instant), `dd-MMM-yyyy` (date-only), `dd-MMM` (compact
  contexts). `"g"`, `"d"` and `"MMM dd, yyyy"` are culture-dependent and prohibited in renders.
- Date-only values are calendar days. Never convert one, never compare one against an instant.
- Server interchange formats stay UTC and carry `CultureInfo.InvariantCulture`.

### Version floor (CRMX-001)
- If this extension uses SDK surface newer than the CRM core it will be installed onto, declare it in
  `ICrmExtension.MinimumCrmVersion` AND state the same floor in the release notes / `Description`.
  Both are required: the code check is enforced only by cores that already contain it, so it is
  forward-only and cannot warn an older core. A refused extension is reported in the Extension
  Manager and keeps its custom field data.

## This extension

Recorded deviations and hard rules specific to Meeting Notes. Empty would mean full compliance.

- No `Resources\` set and no `wwwroot\` assets. Adding user-facing strings means adding the whole
  five-file resx set (CRMX-008); adding styling or script means a new `wwwroot\` in this repo with
  prefixed classes only (CRMX-009).
- Timestamps the extension stores are instants and belong in UTC. If a calendar day is ever stored,
  do not convert it and do not compare it against an instant.
- The post-build packaging step runs `Package\release.cmd`; there is no `debug.cmd` hook here.
