using BYUNotes.Web.Components;
using BYUNotes.Web.Data;
using BYUNotes.Web.Models;
using BYUNotes.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<NotesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("NotesDatabase")));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<ExportNoteService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//runs existing migration scripts if they haven't been applied yet
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<NotesDbContext>>();

    using var db = factory.CreateDbContext();
    db.Database.Migrate();
}
app.Run();
