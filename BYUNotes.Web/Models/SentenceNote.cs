
namespace BYUNotes.Web.Models;

public class SentenceNote : Note
{
    public string SentencesJson { get; set; } = "[]";
}

public class SentenceEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Content { get; set; } = "";
}
