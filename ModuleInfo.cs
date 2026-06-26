using Oqtane.Models;
using Oqtane.Modules;
using StudioElf.Module.CRM;
using StudioElf.Module.CRM.MeetingNotes.Models;

namespace StudioElf.Module.CRM.MeetingNotes;

/// <summary>
/// Oqtane IModule registration — required for Oqtane to discover this assembly
/// as an installable module and trigger migration via ReleaseVersions.
/// Oqtane scans all assemblies in the bin directory for IModule implementations.
/// </summary>
public class ModuleInfo : IModule
{
    public ModuleDefinition ModuleDefinition => new ModuleDefinition
    {
        Name = MeetingNotesModuleInfo.DisplayName,
        Description = MeetingNotesModuleInfo.Description,
        Categories = "Headless",
        Version = MeetingNotesModuleInfo.Version,
        ReleaseVersions = VersionInfo.Version,
        ServerManagerType = "StudioElf.Module.CRM.MeetingNotes.Manager.MeetingNotesManager, StudioElf.Module.CRM.MeetingNotes.Oqtane",
        Dependencies = "StudioElf.Module.CRM.Shared.Oqtane",
        PackageName = "StudioElf.Module.CRM.MeetingNotes"
    };
}