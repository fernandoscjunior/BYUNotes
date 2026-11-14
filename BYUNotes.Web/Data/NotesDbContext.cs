
using BYUNotes.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BYUNotes.Web.Data;

public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options)
        : base(options)
    {
    }

    public DbSet<StudentNote> Notes { get; set; } = null!;
}
