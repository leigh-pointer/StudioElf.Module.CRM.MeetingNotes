using System.Collections.Generic;
using System.Threading.Tasks;
using StudioElf.Module.CRM.MeetingNotes.Models;

namespace StudioElf.Module.CRM.MeetingNotes.Services;

public interface IMeetingNotesService
{
    Task<List<MeetingNoteDto>> GetAllAsync(int moduleId);
    Task<List<MeetingNoteDto>> GetByContactAsync(int contactId, int moduleId);
    Task<MeetingNoteDto> CreateAsync(CreateMeetingNoteDto dto, int moduleId, string createdBy);
    Task<MeetingNoteDto> UpdateAsync(int id, CreateMeetingNoteDto dto, int moduleId, string createdBy);
    Task DeleteAsync(int id, int moduleId);
    Task<List<MeetingNoteDto>> GetRecentAsync(int moduleId, string createdBy, int count = 5);
}
