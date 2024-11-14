using CCMS3.Controllers;
using CCMS3.Data;
using CCMS3.Eceptions;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Interfaces;
using Serilog;

namespace CCMS3.Services.Implementations
{
    public class CreditCardService : ICreditCardService
    {

        private readonly ICreditCardRepository _creditCardRepository;

        public CreditCardService(
            ICreditCardRepository creditCardRepository
            )
        {
            _creditCardRepository = creditCardRepository;

        }
        public CreditCardResponse CreateCreditCard(CreditCardRequest request)
        {

            if (_creditCardRepository.GetCreditCardByPersonalDetailsId(request.PersonalDetailsId) != null)
            {
                throw new AlreadyHasCreditCardException("User already has a credit card!!");
            }
            var creditCard = new CreditCard
            {
                CardHolderName = request.CardHolderName,
                Balance = request.Balance,
                CardNumber = request.CardNumber,
                CreditLimit = request.CreditLimit,
                CVV = request.CVV,
                ExpirationDate = request.ExpirationDate,
                InterestRate = request.InterestRate,
                IssuedDate = request.IssuedDate,
                PersonalDetailsId = request.PersonalDetailsId
            };

            try
            {
                var newCreditCard = _creditCardRepository.CreateCreditCard(creditCard);
                var response = new CreditCardResponse
                {
                    Id = newCreditCard.Id,
                    CardHolderName = newCreditCard.CardHolderName,
                    CardNumber = newCreditCard.CardNumber,
                    Balance = newCreditCard.Balance,
                    CreditLimit = newCreditCard.CreditLimit,
                    CVV = newCreditCard.CVV,
                    ExpirationDate = newCreditCard.ExpirationDate,
                    InterestRate = newCreditCard.InterestRate,
                    IssuedDate = newCreditCard.IssuedDate
                };

                return response;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }

        }

        public void DeleteCreditCard(int id)
        {
            try
            {
                var creditCard = _creditCardRepository.GetCreditCardById(id);
                if (creditCard != null)
                    _creditCardRepository.DeleteCreditCard(id);
                else
                    throw new EntityNotFoundException("Credit card Not Found!!");
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }

        public IEnumerable<CreditCardResponse> GetAllCreditCards()
        {
            try
            {
                var cards = _creditCardRepository.GetCreditCards();
                if (cards.Any())
                {
                    return cards.Select(cr => new CreditCardResponse
                    {
                        Id = cr.Id,
                        CardHolderName = cr.CardHolderName,
                        Balance = cr.Balance,
                        CardNumber = cr.CardNumber,
                        CreditLimit = cr.CreditLimit,
                        CVV = cr.CVV,
                        ExpirationDate = cr.ExpirationDate,
                        InterestRate = cr.InterestRate,
                        IssuedDate = cr.IssuedDate
                    });
                }
                else
                {
                    return [];
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }

        public CreditCardResponse GetCreditCard(int id)
        {
            try
            {
                var creditCard = _creditCardRepository.GetCreditCardById(id);
                if (creditCard == null)
                    return null;
                else
                {
                    var response = new CreditCardResponse
                    {
                        Id = creditCard.Id,
                        CardHolderName = creditCard.CardHolderName,
                        CardNumber = creditCard.CardNumber,
                        Balance = creditCard.Balance,
                        CreditLimit = creditCard.CreditLimit,
                        CVV = creditCard.CVV,
                        ExpirationDate = creditCard.ExpirationDate,
                        InterestRate = creditCard.InterestRate,
                        IssuedDate = creditCard.IssuedDate
                    };
                    return response;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }
        }

        public CreditCardResponse UpdateCreditCard(CreditCardRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
