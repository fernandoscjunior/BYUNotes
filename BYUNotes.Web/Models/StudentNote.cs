
namespace BYUNotes.Web.Models;

public class StudentNote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";

    public string Type { get; set; } = "Regular";

    public string Content { get; set; } = "";

    public string Cues { get; set; } = "";

    public string Summary { get; set; } = "";

    public DateTime LastEdited { get; set; } = DateTime.Now;
}
