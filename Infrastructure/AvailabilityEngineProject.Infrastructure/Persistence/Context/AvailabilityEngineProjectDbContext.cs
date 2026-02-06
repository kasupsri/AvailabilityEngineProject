using Microsoft.EntityFrameworkCore;
using AvailabilityEngineProject.Infrastructure.Persistence.Entity;

namespace AvailabilityEngineProject.Infrastructure.Persistence.Context;

public class AvailabilityEngineProjectDbContext : DbContext
{
    public AvailabilityEngineProjectDbContext(DbContextOptions<AvailabilityEngineProjectDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Person> Persons { get; set; }
    public virtual DbSet<PersonBusyInterval> PersonBusyIntervals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Persons_pkey");
            entity.ToTable("Persons");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Email).HasMaxLength(320).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasMany<PersonBusyInterval>()
                .WithOne()
                .HasForeignKey(pbi => pbi.PersonId)
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonBusyInterval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PersonBusyIntervals_pkey");
            entity.ToTable("PersonBusyIntervals");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PersonId).IsRequired();
            entity.Property(e => e.StartUtc).IsRequired();
            entity.Property(e => e.EndUtc).IsRequired();
            entity.HasIndex(e => e.PersonId);
        });
    }
}
