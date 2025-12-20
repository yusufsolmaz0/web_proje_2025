using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ExerciseSuggestion> ExerciseSuggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<Trainer>()
                .HasMany(t => t.Services)
                .WithMany(s => s.Trainers)
                .UsingEntity(j => j.ToTable("TrainerServices"));
        }
    }
}
