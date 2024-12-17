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

        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<SpendAnalysis> SpendAnalyses { get; set; }

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
                .WithMany()
                .HasForeignKey(pd => pd.EmploymentStatusId);

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

            builder.Entity<CreditCard>(
                entity =>
                {
                    entity.HasIndex(c => c.CardNumber).IsUnique();
                    entity.HasIndex(c => c.CVV).IsUnique();
                });

            builder.Entity<Category>(entity =>
            {
                entity.HasData(
                    new Category { Id = 1, CategoryName = "Groceries" },
                    new Category { Id = 2, CategoryName = "Travel" },
                    new Category { Id = 3, CategoryName = "Entertainment" },
                    new Category { Id = 4, CategoryName = "Utilities" },
                    new Category { Id = 5, CategoryName = "Health" },
                    new Category { Id = 6, CategoryName = "Education" },
                    new Category { Id = 7, CategoryName = "Shopping" },
                    new Category { Id = 8, CategoryName = "Dining" },
                    new Category { Id = 9, CategoryName = "Fuel" },
                    new Category { Id = 10, CategoryName = "Rent" }
                    );
            });

            builder.Entity<TransactionType>(
                entity =>
                {
                    entity.HasData(
                        new TransactionType { Id = 1, Type = "Purchase" },
                        new TransactionType { Id = 2, Type = "Payment" }
                        );
                });

        }
    }
}
