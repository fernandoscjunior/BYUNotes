
namespace BYUNotes.Web.Models;

public class MappingNote : Note
{
    public string CentralTopic { get; set; } = "";

    public string BranchesJson { get; set; } = "[]";
}

public class MappingBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public string Color { get; set; } = "#2563eb";

    // Parent branch (null means central topic)
    public Guid? ParentId { get; set; }

    // Position on the mind map canvas
    public double? X { get; set; }

    public double? Y { get; set; }
}
