using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCMS3.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; } // Primary key

        [Precision(18, 2)]
        public decimal Amount { get; set; } // Transaction amount
        public DateTime TransactionDate { get; set; } // Date of the transaction
        public string Description { get; set; } // Description of the transaction (e.g., merchant name)

        [ForeignKey("TransactionType")]
        public int TransactionTypeId { get; set; }
        public virtual TransactionType TransactionType { get; set; } // Type of transaction (e.g., Purchase, Payment, Refund)

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; } // Category of the transaction (e.g., Rent, Fuel, Groceries etc)

        [ForeignKey("CreditCard")]
        public int CreditCardId { get; set; } // Foreign key to CreditCard
        // Navigation property
        public virtual CreditCard CreditCard { get; set; } // Reference to the associated CreditCard
    }
}