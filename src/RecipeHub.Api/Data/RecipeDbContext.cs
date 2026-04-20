using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RecipeHub.Api.Models;

namespace RecipeHub.Api.Data;

public class RecipeDbContext(DbContextOptions<RecipeDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<ShareToken> ShareTokens => Set<ShareToken>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store DateTime values as UTC. SQLite persists DateTime as TEXT; this
        // converter ensures values are written in UTC and tagged DateTimeKind.Utc
        // when read back.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var utcNullableConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue ? v : (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()),
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.Property(r => r.Difficulty)
                .HasConversion<string>()
                .HasMaxLength(16);

            entity.Property(r => r.CreatedAt).HasConversion(utcConverter);
            entity.Property(r => r.UpdatedAt).HasConversion(utcConverter);

            entity.HasMany(r => r.Steps)
                .WithOne(s => s.Recipe!)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.RecipeTags)
                .WithOne(rt => rt.Recipe!)
                .HasForeignKey(rt => rt.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.ShareTokens)
                .WithOne(st => st.Recipe!)
                .HasForeignKey(st => st.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Favorites)
                .WithOne(f => f.Recipe!)
                .HasForeignKey(f => f.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.HasIndex(s => new { s.RecipeId, s.StepNumber }).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(t => t.Name).IsUnique();

            entity.HasMany(t => t.RecipeTags)
                .WithOne(rt => rt.Tag!)
                .HasForeignKey(rt => rt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.HasKey(rt => new { rt.RecipeId, rt.TagId });
        });

        modelBuilder.Entity<ShareToken>(entity =>
        {
            entity.HasIndex(st => st.Token).IsUnique();
            entity.Property(st => st.CreatedAt).HasConversion(utcConverter);
            entity.Property(st => st.ExpiresAt).HasConversion(utcNullableConverter);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.RecipeId }).IsUnique();
            entity.Property(f => f.CreatedAt).HasConversion(utcConverter);
        });
    }
}
