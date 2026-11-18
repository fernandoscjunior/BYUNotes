
namespace BYUNotes.Web.Models;

public class CornellNote : Note
{
    public List<CornellNoteDetail> Cues { get; set; } = [];

    public string Summary { get; set; } = "";

}
