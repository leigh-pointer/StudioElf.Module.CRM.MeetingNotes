# CLAUDE.md - StudioElf CRM Meeting Notes Extension

Instructions for Claude Code and Claude-powered tooling working in **this repository**.

This repository is the **Meeting Notes extension**, not the CRM. It is a separate git
repository that lives inside a CRM extensions workspace. Read this file before any change.

## The Hard Rule: Never Modify the CRM Core

**No file outside this repository may be created, modified, moved, renamed or deleted.**

The CRM core lives at `..\..\..\..\DevStudio.10\StudioElf.Module.CRM` and is a separate
repo. It is read-only from here.

The only tree this repository may write is its own project folder.

Do not touch, for any reason:

| Path | Why |
|------|-----|
| `..\..\..\..\DevStudio.10\StudioElf.Module.CRM\Client\|Server\|Shared\|` | CRM core source |
| `..\..\..\..\DevStudio.10\StudioElf.Module.CRM\Server\wwwroot\Module.css\|Module.js` | CRM core styling and scripts |
| `..\..\docs\governance-crm\` | The synced governance copy - generated, never edited here |
| `..\..\..\..\oqtane.framework\` | Oqtane framework |
| `..\..\..\..\oqtane.framework\Oqtane.Server\bin\**` | Deployed assemblies |
| `..\..\..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\**` | Deployed module assets |

### The Escapes That Are All Violations

Each of these has been reached for. None is acceptable:

- Editing a CRM core component so a Meeting Notes component renders correctly.
- Editing CRM core `Module.css` / `Module.js` to fix Meeting Notes styling or behaviour.
- Widening a CRM core interface, or adding a member to one, because this extension needs more.
- Editing CRM core to clear a compile error this repository caused.
- Editing the deployed `_content` copy or the framework bin copy instead of the source.
- Editing a CRMX rule so that an intended change becomes allowed.
- Copying a CRM core file into this repository and editing the copy.
- Adding a `ProjectReference` from here to a CRM core project to reach an internal type.

### When a CRM Core Change Is Genuinely Required

**STOP.** Report it. Do not implement it, do not propose a patch, and do not leave a
commented-out draft in the tree.

Report, in this order:

1. The core file and member that blocks the work.
2. Why this extension cannot achieve the behaviour without it. Name what was tried.
3. The smallest core change that would unblock it, described - not written.
4. Which other extensions that change would affect.

A core change is a CRM release decision. It ships in the CRM's version, not this
extension's, so nothing in this repository's version or release notes records it.

## Governance

Before generating, modifying, reviewing or refactoring code, read - in this order:

1. `..\..\docs\governance-crm\CRMX-000-index.md` - the CRMX index
2. `..\..\docs\governance-crm\CRMX-013-core-boundary-and-neutrality.md`
3. `.github\module-instructions.md` - accumulated lessons, always
4. The CRMX rule matching the task:
   - `CRMX-001` extension contract - `ICrmExtension`, discovery, deep links, version floor
   - `CRMX-002` SDK versioning - never remove a published signature, never probe the version
   - `CRMX-003` custom fields - declaration only, ownership split
   - `CRMX-004` UI and render modes - `RenderModeBoundary` is absent on widgets, shell, module and user settings
   - `CRMX-005` security - policies, file URLs, generic client errors
   - `CRMX-006` data and migrations - migration numbering, `ReleaseVersions`
   - `CRMX-007` dependencies and packaging
   - `CRMX-008` localization - five resx files, always in sync
   - `CRMX-009` CSS - never style `.crm-*`, prefix everything
   - `CRMX-010` diagnostics - page loads do no work and raise no error
   - `CRMX-011` testing
   - `CRMX-012` AI instruction discovery
   - `CRMX-014` dates and times - UTC storage, display through the inherited helpers
5. Task prompts: `..\..\docs\governance-crm\prompts\`
6. The Playbook: `..\..\..\oqtane-ai-playbook\module-playbook-example\docs\governance\027-rules-index.md`
   and the relevant `..\..\..\oqtane-ai-playbook\module-playbook-example\docs\prompts\*.md`
7. The SDK surface the task uses: `..\..\..\..\DevStudio.10\StudioElf.Module.CRM\docs\StudioElf CRM SDK.md`,
   and confirm every member exists in `..\..\..\..\DevStudio.10\StudioElf.Module.CRM\Shared\Interfaces\`

The governance copy at `..\..\docs\governance-crm\` is **generated** from the canonical set in
the CRM core repo. Never edit a copy - report drift instead. Refresh or check it with
`..\..\sync-crmx-governance.ps1` (`-Verify` reports drift, `-DryRun` shows the plan).

If a rule is not in `CRMX-000`'s index, it does not exist and must not be enforced.
If a CRMX rule and an Oqtane AI Playbook rule appear to conflict, **stop and ask**.
CRMX never relaxes a Playbook rule.

## Repository-Specific Rules

- **Identity:** `Extensions\MeetingNotesExtension.cs` implements `ICrmExtension`. Version
  1.0.0. Assembly name is `StudioElf.Module.CRM.MeetingNotes.Oqtane`.
- **`ReleaseVersions` in `ModuleInfo.cs` gains an entry only when that version runs a DB
  migration.** Migrations are `Migrations\VVMMNNNN_*.cs`.
- **`MinimumCrmVersion`:** not declared today. Declare it only if this extension starts
  using SDK surface newer than the CRM it will be installed onto, and state the same floor
  in the release notes / `Description` as well - the code check is forward-only (CRMX-001).
- **Dates (CRMX-014):** meeting dates and timestamps are instants unless the field is
  explicitly a calendar day. Store UTC, render through the inherited `CrmBase` helpers,
  never re-declare them.
- **No `Resources\` and no `wwwroot\` today.** There are no user-facing strings going
  through a localizer and no CSS or JS. If either is added, follow `CRMX-008` (full
  five-file resx set) or `CRMX-009` (own `wwwroot\`, prefixed classes, never `.crm-*`).
- **Dependencies:** Oqtane and CRM assemblies come by `HintPath`. Framework-owned packages
  (EF, ASP.NET, ImageSharp) must match the framework's pin exactly; module-owned packages
  (Markdig) must match the CRM core's pin.
- **Packaging:** stop the Oqtane server first, or `debug.cmd` / `release.cmd` fail with
  exit code 4 (sharing violation).
- **Never commit or tag without explicit user approval.**
- **ASCII punctuation only** in code, comments and docs: no em dashes, en dashes, smart
  quotes or ellipses. Use `-`, `'`, `"`, `...`.

## Build

```cmd
dotnet build StudioElf.Module.CRM.MeetingNotes.csproj
```

The build runs a post-build packaging step that copies the module dll into the Oqtane
instance. A stale CRM core is the usual cause of a confusing compile error - confirm the
framework bin copies of `StudioElf.Module.CRM.*.Oqtane.dll` are current.
