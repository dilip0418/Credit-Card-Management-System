using System.ComponentModel.DataAnnotations;

namespace CCMS3.Models
{
    public class ApplicationStatus
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Example values: "Applied", "Accepted", "Rejected"

        // Navigation property
        public ICollection<CreditCardApplication> CreditCardApplications { get; set; }
    }
}