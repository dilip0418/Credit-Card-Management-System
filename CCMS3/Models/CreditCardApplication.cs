using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CCMS3.Models
{
    public class CreditCardApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        // Foreign key to PersonalDetails
        [ForeignKey("PersonalDetails")]
        public string PersonalDetailsId { get; set; }
        public PersonalDetails PersonalDetails { get; set; } // Navigation property

        // Foreign key to ApplicationStatus
        [ForeignKey("ApplicationStatus")]
        public int ApplicationStatusId { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; } // Navigation property

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone, MinLength(10, ErrorMessage ="Phone number must be of 10 digits"), MaxLength(10, ErrorMessage ="Phone number must be of 10 digits")]
        public string PhoneNo { get; set; }

        public string Comments { get; set; } // Comments from admin/verifier

        public decimal AnnualIncome { get; set; }
    }
}
