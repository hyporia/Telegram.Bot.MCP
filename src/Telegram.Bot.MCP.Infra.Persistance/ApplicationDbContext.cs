using Microsoft.EntityFrameworkCore;
using Telegram.Bot.MCP.Domain;

namespace Telegram.Bot.MCP.Infra.Persistance;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.FirstName)
                  .HasMaxLength(100);
            entity.Property(e => e.LastName)
                  .HasMaxLength(100);

            entity.HasIndex(e => e.Username);

            entity.HasIndex(e => e.IsAdmin);

            entity.HasIndex(e => e.IsMe)
                .HasFilter("IsMe = 1")
                .IsUnique();

            entity.HasMany(u => u.Groups)
                .WithMany(g => g.Users)
                .UsingEntity(j => j.ToTable("UserGroups"));
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text)
                  .IsRequired()
                  .HasMaxLength(4096);

            entity.Property(e => e.Timestamp)
                  .IsRequired()
                  .HasColumnType("datetime2");

            entity.HasIndex(e => e.Timestamp);

            entity.HasIndex(e => e.UserId);

            entity.HasIndex(e => new { e.UserId, e.Timestamp });

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(e => e.Name)
                  .IsUnique();
        });
    }
}