
using System.Linq;
using System.Text;
using System.Text.Json;
using BYUNotes.Web.Models;
using Microsoft.JSInterop;

namespace BYUNotes.Web.Services;

public class ExportNoteService(IJSRuntime JS)
{
    public async Task ExportNoteAsync(string format, Note? selectedNote)
    {
        if (selectedNote is null)
            return;
        format = format.Trim().ToLowerInvariant();

        // Existing text-based exports
        if (format == "txt" || format == "md")
        {
            await ExportNoteAsync(format == "md", selectedNote);
            return;
        }

        // Validate visual export format
        if (format != "png" &&
            format != "pdf" &&
            format != "html")
        {
            throw new ArgumentException(
                $"Unsupported export format: {format}");
        }

        var safeTitle = string.Join("_",
            selectedNote.Title.Split(Path.GetInvalidFileNameChars()));

        if (string.IsNullOrWhiteSpace(safeTitle))
            safeTitle = "Untitled_Note";

        // Visual exports
        await JS.InvokeVoidAsync(
            "noteExport.download",
            "note-export-preview",
            safeTitle,
            format);
    }
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

        var builder = new StringBuilder();

        builder.AppendLine(markdown
            ? $"# {note.Title}"
            : note.Title);

        builder.AppendLine();

        // CORNELL NOTES
        if (note is CornellNote cornellNote)
        {
            foreach (var cue in cornellNote.Cues
                .OrderBy(c => c.CueSequence))
            {
                builder.AppendLine(markdown
                    ? $"## {cue.Cue}"
                    : $"Cue / Question: {cue.Cue}");

                builder.AppendLine();

                builder.AppendLine(markdown
                    ? cue.Content
                    : $"Main Notes: {cue.Content}");

                builder.AppendLine();
            }

            builder.AppendLine(markdown
                ? "## Summary"
                : "Summary:");

            builder.AppendLine(cornellNote.Summary);
        }

        // TEXT NOTES
        else if (note is TextNote textNote)
        {
            builder.AppendLine(textNote.Content);
        }

        // OUTLINE NOTES
        else if (note is OutlineNote outlineNote)
        {
            builder.AppendLine(outlineNote.Content);
        }

        // MAPPING / MIND MAP NOTES
        else if (note is MappingNote mappingNote)
        {
            builder.AppendLine(markdown
                ? $"## Central Topic: {mappingNote.CentralTopic}"
                : $"Central Topic: {mappingNote.CentralTopic}");

            builder.AppendLine();

            var branches =
                JsonSerializer.Deserialize<List<MappingBranch>>(
                    string.IsNullOrWhiteSpace(mappingNote.BranchesJson)
                        ? "[]"
                        : mappingNote.BranchesJson)
                ?? new List<MappingBranch>();

            // Exclude central topic position and color settings.
            branches = branches
                .Where(b => b.Id != Guid.Empty)
                .ToList();

            for (int i = 0; i < branches.Count; i++)
            {
                var branch = branches[i];

                var parent = branches.FirstOrDefault(
                    b => b.Id == branch.ParentId);

                var parentTitle = parent is null
                    ? mappingNote.CentralTopic
                    : parent.Title;

                var branchTitle =
                    string.IsNullOrWhiteSpace(branch.Title)
                        ? "Untitled Branch"
                        : branch.Title;

                builder.AppendLine(markdown
                    ? $"### {branchTitle}"
                    : $"Branch {i + 1}: {branchTitle}");

                builder.AppendLine(markdown
                    ? $"**Connected to:** {parentTitle}"
                    : $"Connected to: {parentTitle}");

                builder.AppendLine();

                builder.AppendLine(markdown
                    ? branch.Content
                    : $"Description: {branch.Content}");

                builder.AppendLine();
            }
        }

        var content = builder.ToString();

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
