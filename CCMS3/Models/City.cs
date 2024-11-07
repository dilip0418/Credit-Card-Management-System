using System.ComponentModel.DataAnnotations.Schema;

namespace CCMS3.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("State")]
        public int? StateId { get; set; }
        public State State { get; set; }
    }

    public class ApiCity
    {
        public string name { get; set; }
    }
}