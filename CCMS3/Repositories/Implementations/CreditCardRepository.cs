using CCMS3.Data;
using CCMS3.Eceptions;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CCMS3.Repositories.Implementations
{
    public class CreditCardRepository : ICreditCardRepository
    {


        private readonly IPersonalDetailsRepository _personalDetailsRepository;
        private readonly AppDbContext _context;


        public CreditCardRepository(
            IPersonalDetailsRepository personalDetailsRepository,
            AppDbContext context
            )
        {
            _personalDetailsRepository = personalDetailsRepository;
            _context = context;
        }
        public CreditCard CreateCreditCard(CreditCard creditCard)
        {
            try
            {
                var personalDetails = _personalDetailsRepository.GetPersonalDetailsById(creditCard.PersonalDetailsId) ?? throw new EntityNotFoundException("PersonalDetail object Not found!!");

                creditCard.PersonalDetails = personalDetails;

                _context.CreditCards.Add(creditCard);
                _context.SaveChanges();

                return creditCard;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }

        }

        public void DeleteCreditCard(int id)
        {
            var card = _context.CreditCards.Find(id) ?? throw new EntityNotFoundException("No Card found apply for one.");

            try
            {
                _context.Remove(card);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }

        public CreditCard GetCreditCardById(int id)
        {
            try
            {

                return _context.CreditCards
                    .Include(u => u.PersonalDetails)
                    .FirstOrDefault(c => c.Id == id)!;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }

        public CreditCard GetCreditCardByPersonalDetailsId(string id)
        {
            return _context.CreditCards
                .Include(t => t.Transactions)
                .FirstOrDefault(c => c.PersonalDetailsId.Equals(id));
        }

        public IEnumerable<CreditCard> GetCreditCards()
        {
            try
            {
                var creditCards = _context.CreditCards
                    .Include(u => u.PersonalDetails);
                if (creditCards.Any())
                    return creditCards;
                else
                    return [];

            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }


        }

        public async Task UpdateBalanceAsync(int cardId, int type, decimal amount)
        {
            var card = await _context.CreditCards.FindAsync(cardId);
            if(type == 2)
            {
                card.Balance -= amount;
            }
            else
            {
                card.Balance += amount;
            }
            _context.Update(card);
            await _context.SaveChangesAsync();
        }

        public CreditCard UpdateCreditCard(CreditCard creditCard)
        {
            throw new NotImplementedException();
        }
    }
}
