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

        [Required, Phone]
        public string PhoneNo { get; set; }

        public string Comments { get; set; } // Comments from admin/verifier
    }
}
