using BYUNotes.Web.Data;
using BYUNotes.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BYUNotes.Web.Services;
public class NoteService(IDbContextFactory<NotesDbContext> factory)
{
    public async Task<List<Note>> LoadNotesAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Notes
            .Include(n => (n as CornellNote)!.Cues)
            .OrderByDescending(n => n.LastEdited)
            .ToListAsync();
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
        //Need to handle new details 
        if (note is CornellNote cornellNote)
        {
            var existingNote = await db.Notes
                .OfType<CornellNote>()
                .Include(n => n.Cues)
                .FirstOrDefaultAsync(n => n.Id == note.Id);

            if (existingNote is null)
                return;

            // Update the note itself
            db.Entry(existingNote).CurrentValues.SetValues(cornellNote);

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
            db.Notes.Update(note);
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteNoteAsync(Note note)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Notes.Remove(note);
        await db.SaveChangesAsync();
    }
}