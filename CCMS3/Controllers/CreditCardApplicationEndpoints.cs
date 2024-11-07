namespace CCMS3.Controllers
{

    public class CreditCardApplicationRequest
    {

    }

    public class CreditCardApplicationResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime MyProperty { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public int ApplicationStatusId { get; set; }
        public string ApplicationStatus { get; set; }
        public decimal AnnualIncome { get; set; }

    }
    public static class CreditCardApplicationEndpoints
    {

    }
}
