using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CCMS3.Models
{
    public class PersonalDetails
    {
        [Key, ForeignKey("User")]
        public string UserId { get; set; }

        public DateOnly DateOfBirth { get; set; }

        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public Address Address { get; set; }

        public AppUser User { get; set; }

        public int EmploymentStatusId { get; set; }
        public EmploymentStatus EmploymentStatus { get; set; }

        public decimal AnnualIncome { get; set; } = 0.0M;
    }
}