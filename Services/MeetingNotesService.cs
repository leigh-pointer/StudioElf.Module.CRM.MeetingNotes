using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudioElf.Module.CRM.MeetingNotes.Models;
using StudioElf.Module.CRM.MeetingNotes.Repository;

namespace StudioElf.Module.CRM.MeetingNotes.Services;

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

    public async Task<List<MeetingNoteDto>> GetAllAsync(int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.MeetingNotes
            .Where(m => m.ModuleId == moduleId)
            .OrderByDescending(m => m.MeetingDate)
            .Select(m => ToDto(m))
            .ToListAsync();
    }

    public async Task<MeetingNoteDto> CreateAsync(CreateMeetingNoteDto dto, int moduleId, string createdBy)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entity = new MeetingNote
        {
            ModuleId = moduleId,
            ContactId = dto.ContactId,
            CompanyId = dto.CompanyId,
            DealId = dto.DealId,
            Title = dto.Title,
            Summary = dto.Summary,
            MeetingDate = dto.MeetingDate,
            DurationMinutes = dto.DurationMinutes,
            Location = dto.Location,
            ActionItems = dto.ActionItems,
            Attendees = dto.Attendees,
            CreatedBy = createdBy,
            CreatedOn = DateTime.UtcNow
        };
        db.MeetingNotes.Add(entity);
        await db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<MeetingNoteDto> UpdateAsync(int id, CreateMeetingNoteDto dto, int moduleId, string createdBy)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entity = await db.MeetingNotes
            .FirstOrDefaultAsync(m => m.Id == id && m.ModuleId == moduleId);
        if (entity == null)
            throw new KeyNotFoundException($"MeetingNote {id} not found");

        entity.Title = dto.Title;
        entity.Summary = dto.Summary;
        entity.MeetingDate = dto.MeetingDate;
        entity.DurationMinutes = dto.DurationMinutes;
        entity.Location = dto.Location;
        entity.ActionItems = dto.ActionItems;
        entity.Attendees = dto.Attendees;
        entity.ModifiedBy = createdBy;
        entity.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, int moduleId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entity = await db.MeetingNotes
            .FirstOrDefaultAsync(m => m.Id == id && m.ModuleId == moduleId);
        if (entity != null)
        {
            db.MeetingNotes.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<MeetingNoteDto>> GetRecentAsync(int moduleId, string createdBy, int count = 5)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.MeetingNotes
            .Where(m => m.ModuleId == moduleId && m.CreatedBy == createdBy)
            .OrderByDescending(m => m.MeetingDate)
            .Take(count)
            .Select(m => ToDto(m))
            .ToListAsync();
    }

    private static MeetingNoteDto ToDto(MeetingNote m) => new()
    {
        Id = m.Id,
        ContactId = m.ContactId,
        CompanyId = m.CompanyId,
        DealId = m.DealId,
        Title = m.Title,
        Summary = m.Summary,
        MeetingDate = m.MeetingDate,
        DurationMinutes = m.DurationMinutes,
        Location = m.Location,
        ActionItems = m.ActionItems,
        Attendees = m.Attendees,
        CreatedOn = m.CreatedOn,
        ModifiedOn = m.ModifiedOn
    };
}
