using BYUNotes.Web.Models;
using Microsoft.JSInterop;

namespace BYUNotes.Web.Services;

public class ExportNoteService(IJSRuntime JS)
{
    public async Task ExportNoteAsync(bool markdown, Note? selectedNote)
    {
        if (selectedNote is null)
            return;

        var note = selectedNote;
        var extension = markdown ? ".md" : ".txt";

        var safeTitle = string.Join("_",
            note.Title.Split(Path.GetInvalidFileNameChars()));

        if (string.IsNullOrWhiteSpace(safeTitle))
            safeTitle = "Untitled_Note";

        string content = "";

        if (note is CornellNote cornellNote)
        {
            content = markdown
                ? $"# {note.Title}\n\n" +
                  $"## Cues / Questions\n{cornellNote.Cues}\n\n" +
                  //$"## Main Notes\n{note.Content}\n\n" +
                  $"## Summary\n{cornellNote.Summary}\n"
                : $"{note.Title}\n\n" +
                  $"Cues / Questions:\n{cornellNote.Cues}\n\n" +
                  //$"Main Notes:\n{note.Content}\n\n" +
                  $"Summary:\n{cornellNote.Summary}\n";
        }
        else if (note is TextNote textNote)
        {
            //TODO Fix
            content = markdown
                ? $"# {textNote.Title}\n\n{textNote.Title}\n"
                : $"{textNote.Content}\n\n{textNote.Content}\n";
        }

        var contentType = markdown
            ? "text/markdown;charset=utf-8"
            : "text/plain;charset=utf-8";

        await JS.InvokeVoidAsync(
            "downloadNote",
            safeTitle + extension,
            content,
            contentType);
    }
}