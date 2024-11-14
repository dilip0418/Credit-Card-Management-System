using CCMS3.Models;

namespace CCMS3.Repositories.Interfaces
{
    public interface ICreditCardRepository
    {
        public CreditCard GetCreditCardById(int id);
        public CreditCard GetCreditCardByPersonalDetailsId(string id);
        public IEnumerable<CreditCard> GetCreditCards();

        public CreditCard UpdateCreditCard(CreditCard creditCard);
        public void DeleteCreditCard(int id);
        public CreditCard CreateCreditCard(CreditCard creditCard);
    }
}
