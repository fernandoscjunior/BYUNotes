
using BYUNotes.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BYUNotes.Web.Data;

public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CornellNote>();
        modelBuilder.Entity<TextNote>();

    }
}
