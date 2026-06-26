using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace StudioElf.Module.CRM.MeetingNotes.Models;

/// <summary>
/// MeetingNote entity — stored in extension's own database table.
/// Extends ModelBase for standard audit fields (CreatedBy, ModifiedBy, etc.).
/// </summary>
[Table("MeetingNote")]
public class MeetingNote : ModelBase
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int ContactId { get; set; }
    public int? CompanyId { get; set; }
    public int? DealId { get; set; }

    [Required, MaxLength(500)]
    public string Title { get; set; }
    public string Summary { get; set; }
    public DateTime MeetingDate { get; set; }
    public int DurationMinutes { get; set; }
    [MaxLength(500)]
    public string Location { get; set; }
    /// <summary>JSON array of { text, assignedTo, dueDate, completed }</summary>
    public string ActionItems { get; set; }
    /// <summary>JSON array of attendee names/emails</summary>
    public string Attendees { get; set; }
}

public class MeetingNoteDto
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int? CompanyId { get; set; }
    public int? DealId { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public DateTime MeetingDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Location { get; set; }
    public string ActionItems { get; set; }
    public string Attendees { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class CreateMeetingNoteDto
{
    public int ContactId { get; set; }
    public int? CompanyId { get; set; }
    public int? DealId { get; set; }
    [Required, MaxLength(500)]
    public string Title { get; set; }
    public string Summary { get; set; }
    public DateTime MeetingDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Location { get; set; }
    public string ActionItems { get; set; }
    public string Attendees { get; set; }
}
