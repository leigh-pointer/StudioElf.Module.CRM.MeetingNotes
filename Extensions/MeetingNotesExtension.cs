using System;
using System.Collections.Generic;
using StudioElf.Module.CRM.Models;
using StudioElf.Module.CRM.Services;
using StudioElf.Module.CRM.MeetingNotes.Models;

namespace StudioElf.Module.CRM.Extensions;

/// <summary>
/// ICrmExtension contract. No constructor params, no DI — pure metadata provider.
/// All values read from MeetingNotesModuleInfo constants. Factory: sp => new MeetingNotesExtension().
/// </summary>
public class MeetingNotesExtension : ICrmExtension
{
    public string ExtensionId => MeetingNotesModuleInfo.ExtensionId;
    public string DisplayName => MeetingNotesModuleInfo.DisplayName;
    public string Description => MeetingNotesModuleInfo.Description;
    public string Version => MeetingNotesModuleInfo.Version;
    public string IconClass => MeetingNotesModuleInfo.IconClass;

    public List<CrmNavItem> GetNavItems() => new();
    public List<CrmDashboardWidget> GetDashboardWidgets() => new()
    {
        new("recent-meetings", "Recent Meetings", typeof(RecentMeetingsWidget), 10)
    };
    public List<CrmContactTab> GetContactTabs() => new();
    public List<CrmEmailTemplate> GetEmailTemplates() => new()
    {
        new("Meeting Summary", "Meeting Summary: {{MeetingTitle}}",
            "A meeting was held on {{MeetingDate}} with {{AttendeeCount}} attendees.\n\n{{Summary}}\n\nAction Items:\n{{ActionItems}}")
    };
    public Type GetShellComponentType() => typeof(MeetingNotesShell);
    public List<TimelineItem> GetTimelineItems(string entityName, int entityId, int moduleId, TimelineFilter filter) => null;
}