using System.ComponentModel.DataAnnotations.Schema;

namespace CCMS3.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }

        [ForeignKey("City")]
        public int CityId { get; set; }
        public City City { get; set; }

        [ForeignKey("State")]
        public int StateId { get; set; }
        public State State { get; set; }
    }
}