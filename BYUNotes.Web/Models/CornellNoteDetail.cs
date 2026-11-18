
namespace BYUNotes.Web.Models;

//Keep Id to distinguish the note
//Add SEQ to allow it to be ordered differently
public class CornellNoteDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int CueSequence { get; set; } = 1;

    public string Cue { get; set; } = "";

    public string Content { get; set; } = "";

}
