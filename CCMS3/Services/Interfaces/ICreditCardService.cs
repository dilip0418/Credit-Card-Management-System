using CCMS3.Controllers;

namespace CCMS3.Services.Interfaces
{
    public interface ICreditCardService
    {
        public CreditCardResponse GetCreditCard(int id);
        public CreditCardResponse CreateCreditCard(CreditCardRequest request);
        public CreditCardResponse UpdateCreditCard(CreditCardRequest request);
        public void DeleteCreditCard(int id);
        public IEnumerable<CreditCardResponse> GetAllCreditCards();
    }
}
