using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCMS3.Models
{
    public class SpendAnalysis
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PersonalDetails")]
        public string PersonalDetailsId { get; set; }

        public virtual PersonalDetails PersonalDetails { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; } // Reference to Category
        public virtual Category Category { get; set; }

        public int Year { get; set; } // Year of analysis
        public int Month { get; set; } // Month of analysis

        [Precision(18, 2)]
        public decimal TotalSpend { get; set; } // Aggregated spending amount
    }
}
