namespace BYUNotes.Web.Models;

public class Label
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public string Color { get; set; } = "#0d6efd";

    public List<NoteLabel> NoteLabels { get; set; } = [];
}
