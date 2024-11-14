using System.ComponentModel.DataAnnotations;

namespace CCMS3.Models
{
    public class TransactionType
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }
}