using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Interfaces;
using Serilog;

namespace CCMS3.Services.Implementations
{
    public class TransactionsService : ITransactionsService
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly ICreditCardRepository _creditCardRepository;

        public TransactionsService(
            ITransactionsRepository transactionsRepository,
            ICreditCardRepository creditCardRepository)
        {
            _transactionsRepository = transactionsRepository;
            _creditCardRepository = creditCardRepository;
        }
        public async Task<TransactionRepsonse> CreateTransaction(TransactionRequest request)
        {
            var response = _transactionsRepository.CreateTransactionAsync(request).Result;
            if (response != null)
            {
                // update the balance in the credit card
                await _creditCardRepository.UpdateBalanceAsync(response.CreditCardId, response.TransactionTypeId, response.Amount);

                return new TransactionRepsonse
                {
                    Amount = response.Amount,
                    CategoryName = response.Category.CategoryName,
                    CatId = response.CategoryId,
                    CreditCardHolderName = response.CreditCard.PersonalDetails.User.FullName,
                    CredtiCardId = response.CreditCardId,
                    Descritption = response.Description,
                    TransactionId = response.TransactionId,
                    TTID = response.TransactionTypeId,
                    TTName = response.TransactionType.Type
                };
            }
            else
            {
                return null;
            }
        }

        public async Task<PagedResponse<TransactionRepsonse>> GetPagedTransactions(TransactionParams transactionParams)
        {
            var (totalRecords, trasactions) = await _transactionsRepository.GetTransactionPaged(transactionParams);
            if (totalRecords > 0)
            {
                return new PagedResponse<TransactionRepsonse>(trasactions, totalRecords, transactionParams.PageNumber, transactionParams.PageSize);
            }
            else
            {
                return new PagedResponse<TransactionRepsonse>([], 0, transactionParams.PageNumber, totalRecords);
            }
        }

        public IEnumerable<TransactionRepsonse> GetTransactions()
        {
            try
            {
                var transactions = _transactionsRepository.GetTransactions();

                if (transactions.Any())
                {
                    return transactions;
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }

        }
    }
}
