
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

        // CHARTING NOTES
        else if (note is ChartingNote chartingNote)
        {
            var columns =
                JsonSerializer.Deserialize<List<string>>(
                    string.IsNullOrWhiteSpace(chartingNote.ColumnsJson)
                        ? "[]"
                        : chartingNote.ColumnsJson)
                ?? new List<string>();

            var rows =
                JsonSerializer.Deserialize<List<ChartingRow>>(
                    string.IsNullOrWhiteSpace(chartingNote.RowsJson)
                        ? "[]"
                        : chartingNote.RowsJson)
                ?? new List<ChartingRow>();

            // Provide default headings for an empty chart.
            if (columns.Count == 0)
            {
                columns = new List<string>
                {
                    "Topic",
                    "Definition",
                    "Example"
                };
            }

            // Replace empty column headings.
            for (int i = 0; i < columns.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(columns[i]))
                {
                    columns[i] = $"Column {i + 1}";
                }
            }

            if (markdown)
            {
                // MARKDOWN TABLE HEADER
                builder.AppendLine(
                    "| " +
                    string.Join(" | ",
                        columns.Select(EscapeMarkdownCell)) +
                    " |");

                // MARKDOWN TABLE SEPARATOR
                builder.AppendLine(
                    "| " +
                    string.Join(" | ",
                        Enumerable.Repeat("---", columns.Count)) +
                    " |");

                // MARKDOWN TABLE ROWS
                foreach (var row in rows)
                {
                    var cells = new List<string>();

                    for (int i = 0; i < columns.Count; i++)
                    {
                        var value = i < row.Cells.Count
                            ? row.Cells[i]
                            : "";

                        cells.Add(EscapeMarkdownCell(value));
                    }

                    builder.AppendLine(
                        "| " +
                        string.Join(" | ", cells) +
                        " |");
                }

                if (rows.Count == 0)
                {
                    builder.AppendLine();
                    builder.AppendLine("No chart entries.");
                }
            }
            else
            {
                // PLAIN TEXT EXPORT
                builder.AppendLine("CHARTING NOTES");
                builder.AppendLine(
                    new string('=', 50));

                builder.AppendLine();

                builder.AppendLine(
                    $"Columns: {string.Join(", ", columns)}");

                builder.AppendLine(
                    $"Total rows: {rows.Count}");

                builder.AppendLine();

                for (int r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];

                    builder.AppendLine($"ROW {r + 1}");

                    builder.AppendLine(
                        new string('-', 40));

                    for (int c = 0; c < columns.Count; c++)
                    {
                        var value = c < row.Cells.Count
                            ? row.Cells[c]
                            : "";

                        builder.AppendLine(
                            $"{columns[c]}: {value}");
                    }

                    builder.AppendLine();
                }

                if (rows.Count == 0)
                {
                    builder.AppendLine("No chart entries.");
                }
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

    // Prevent special characters from breaking Markdown tables.
    private static string EscapeMarkdownCell(string? value)
    {
        return (value ?? "")
            .Replace("\\", "\\\\")
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("|", "\\|")
            .Replace("\r\n", "<br>")
            .Replace("\n", "<br>")
            .Replace("\r", "<br>");
    }
}
