
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

    public DbSet<Label> Labels { get; set; } = null!;

    public DbSet<NoteLabel> NoteLabels { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NoteLabel>()
            .HasKey(noteLabel => new { noteLabel.NoteId, noteLabel.LabelId });

        modelBuilder.Entity<NoteLabel>()
            .HasOne(noteLabel => noteLabel.Note)
            .WithMany(note => note.NoteLabels)
            .HasForeignKey(noteLabel => noteLabel.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NoteLabel>()
            .HasOne(noteLabel => noteLabel.Label)
            .WithMany(label => label.NoteLabels)
            .HasForeignKey(noteLabel => noteLabel.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CornellNote>();

        modelBuilder.Entity<TextNote>()
            .Property(n => n.Content)
            .HasColumnName("Content");

        modelBuilder.Entity<OutlineNote>()
            .Property(n => n.Content)
            .HasColumnName("Content");

        modelBuilder.Entity<MappingNote>();

        modelBuilder.Entity<ChartingNote>();
    }
}

