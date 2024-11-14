using System.ComponentModel.DataAnnotations;

namespace CCMS3.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }
    }
}