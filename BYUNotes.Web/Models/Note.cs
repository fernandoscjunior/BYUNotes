
namespace BYUNotes.Web.Models;

public abstract class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";

    public string Subject { get; set; } = "";

    public List<NoteLabel> NoteLabels { get; set; } = [];

    //public string Type { get; set; } = "Regular";

    public DateTime LastEdited { get; set; } = DateTime.Now;
}
