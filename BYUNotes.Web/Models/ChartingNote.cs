namespace BYUNotes.Web.Models;

public class ChartingNote : Note
{
    public string ColumnsJson { get; set; } = "[]";

    public string RowsJson { get; set; } = "[]";
}

public class ChartingRow
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public List<string> Cells { get; set; } = new();
}