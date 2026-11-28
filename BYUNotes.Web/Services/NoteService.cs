using BYUNotes.Web.Data;
using BYUNotes.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BYUNotes.Web.Services;
public class NoteService(IDbContextFactory<NotesDbContext> factory)
{
    public async Task<List<Note>> LoadNotesAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Notes
            .AsSplitQuery()
            .Include(n => (n as CornellNote)!.Cues)
            .Include(n => n.NoteLabels)
                .ThenInclude(noteLabel => noteLabel.Label)
            .OrderByDescending(n => n.LastEdited)
            .ToListAsync();
    }

    public async Task<List<Label>> LoadLabelsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Labels
            .OrderBy(label => label.Name)
            .ToListAsync();
    }

    public async Task<Label> CreateLabelAsync(string name, string color)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
            throw new ArgumentException("Label name cannot be empty.", nameof(name));

        if (!Regex.IsMatch(color, "^#[0-9a-fA-F]{6}$"))
            throw new ArgumentException("Label color must be a six-digit hex color.", nameof(color));

        await using var db = await factory.CreateDbContextAsync();
        if (await db.Labels.AnyAsync(label => label.Name.ToLower() == normalizedName.ToLower()))
            throw new InvalidOperationException("A label with that name already exists.");

        var label = new Label
        {
            Name = normalizedName,
            Color = color
        };

        db.Labels.Add(label);
        await db.SaveChangesAsync();
        return label;
    }

    public async Task SetNoteLabelsAsync(Guid noteId, IEnumerable<Guid> labelIds)
    {
        await using var db = await factory.CreateDbContextAsync();

        var note = await db.Notes
            .Include(existingNote => existingNote.NoteLabels)
            .FirstOrDefaultAsync(existingNote => existingNote.Id == noteId);

        if (note is null)
            throw new InvalidOperationException("The note no longer exists.");

        var requestedIds = labelIds.Distinct().ToHashSet();
        var validIds = await db.Labels
            .Where(label => requestedIds.Contains(label.Id))
            .Select(label => label.Id)
            .ToListAsync();

        if (validIds.Count != requestedIds.Count)
            throw new InvalidOperationException("One or more selected labels no longer exist.");

        foreach (var existing in note.NoteLabels.ToList())
        {
            if (!requestedIds.Contains(existing.LabelId))
                note.NoteLabels.Remove(existing);
        }

        var existingIds = note.NoteLabels.Select(noteLabel => noteLabel.LabelId).ToHashSet();
        foreach (var labelId in requestedIds)
        {
            if (!existingIds.Contains(labelId))
                note.NoteLabels.Add(new NoteLabel { NoteId = noteId, LabelId = labelId });
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteLabelAsync(Guid labelId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var label = await db.Labels.FindAsync(labelId);
        if (label is null)
            throw new InvalidOperationException("The label no longer exists.");

        db.Labels.Remove(label);
        await db.SaveChangesAsync();
    }

    public async Task CreateNoteAsync(Note note)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Notes.Add(note);
        await db.SaveChangesAsync();
    }

    public async Task SaveNoteAsync(Note note)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Notes
            .FirstOrDefaultAsync(existingNote => existingNote.Id == note.Id);

        if (existing is null)
            throw new InvalidOperationException("The note no longer exists.");

        db.Entry(existing).CurrentValues.SetValues(note);

        if (note is CornellNote cornellNote)
        {
            var existingNote = await db.Notes
                .OfType<CornellNote>()
                .Include(n => n.Cues)
                .FirstOrDefaultAsync(n => n.Id == note.Id);

            if (existingNote is null)
                throw new InvalidOperationException("The Cornell note no longer exists.");

            // Delete removed cues
            foreach (var existingCue in existingNote.Cues.ToList())
            {
                if (!cornellNote.Cues.Any(c => c.Id == existingCue.Id))
                {
                    db.Remove(existingCue);
                }
            }

            // Add or update cues
            foreach (var cue in cornellNote.Cues)
            {
                var existingCue = existingNote.Cues
                    .FirstOrDefault(c => c.Id == cue.Id);

                if (existingCue is null)
                {
                    var newCue = new CornellNoteDetail
                    {
                        Id = cue.Id,
                        CueSequence = cue.CueSequence,
                        Cue = cue.Cue,
                        Content = cue.Content
                    };

                    db.Entry(newCue).State = EntityState.Added;
                    existingNote.Cues.Add(newCue);
                }
                else
                {
                    db.Entry(existingCue).CurrentValues.SetValues(cue);
                }
            }

            await db.SaveChangesAsync();
        }
        else
        {
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteNoteAsync(Note note)
    {
        await using var db = await factory.CreateDbContextAsync();

        var existingNote = await db.Notes
            .FirstOrDefaultAsync(existingNote => existingNote.Id == note.Id);

        if (existingNote is null)
            throw new InvalidOperationException("The note no longer exists.");

        db.Notes.Remove(existingNote);
        await db.SaveChangesAsync();
    }
}