using CCMS3.Helpers.Validators;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CCMS3.Models
{
    public class CreditCard
    {
        [Key]
        public int Id { get; set; }

        [CreditCardNumber(ErrorMessage = "Invalid Credit Card number!")]
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public DateTime IssuedDate { get; set; }

        [FutureDate(ErrorMessage = "Expiry date should be a date in future!")]
        public DateTime ExpirationDate { get; set; }
        public int CVV { get; set; }

        [Precision(18, 2)]
        public decimal CreditLimit { get; set; }
        [Precision(18, 2)]
        public decimal Balance { get; set; }
        [Precision(5, 2)]
        public decimal InterestRate { get; set; } = 0.20M; // defaulting to 20%

        [ForeignKey("PersonalDetails")]
        public string PersonalDetailsId { get; set; }
        public PersonalDetails PersonalDetails { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = [];
    }
}
