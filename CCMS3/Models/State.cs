namespace CCMS3.Models
{
    public class State
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StateCode { get; set; }
        public ICollection<City> Cities { get; set; }
    }

    public class ApiState
    {
        public string name { get; set; }
        public string isoCode { get; set; }
    }
}