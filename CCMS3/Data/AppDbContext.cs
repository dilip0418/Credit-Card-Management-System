using CCMS3.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {

        public DbSet<PersonalDetails> PersonalDetails { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        public DbSet<EmploymentStatus> EmploymentStatuses { get; set; }

        public DbSet<CreditCardApplication> CreditCardApplications { get; set; }
        public DbSet<ApplicationStatus> ApplicationStatuses { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AppUser>()
                .HasOne(u => u.PersonalDetails)
                .WithOne(pd => pd.User)
                .HasForeignKey<PersonalDetails>(pd => pd.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting User deletes PersonalDetails

            builder.Entity<PersonalDetails>()
                .HasOne(pd => pd.User)
                .WithOne(u => u.PersonalDetails);

            builder.Entity<PersonalDetails>()
                .HasOne(pd => pd.EmploymentStatus)
                .WithOne()
                .HasForeignKey<PersonalDetails>(pd => pd.EmploymentStatusId);

            builder.Entity<PersonalDetails>()
                .HasOne(pd => pd.Address)
                .WithOne()
                .HasForeignKey<PersonalDetails>(pd => pd.AddressId);


            builder.Entity<EmploymentStatus>()
                .HasData(
                    new EmploymentStatus { Id = 1, Status = "Employed" },
                    new EmploymentStatus { Id = 2, Status = "Unemployed" }
                );

            builder.Entity<Address>()
                .HasOne(a => a.City)
                .WithMany()
                .HasForeignKey(a => a.CityId);

            builder.Entity<Address>()
                .HasOne(a => a.State)
                .WithMany()
                .HasForeignKey(a => a.StateId);

            builder.Entity<State>()
                .HasMany(s => s.Cities)
                .WithOne(c => c.State)
                .HasForeignKey(c => c.StateId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<City>()
                .HasOne(c => c.State)
                .WithMany(s => s.Cities)
                .HasForeignKey(c => c.StateId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
