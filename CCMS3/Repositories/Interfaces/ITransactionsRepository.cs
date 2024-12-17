using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;

namespace CCMS3.Repositories.Interfaces
{
    public interface ITransactionsRepository
    {
        public Task<Transaction> CreateTransactionAsync(TransactionRequest request);
        public IEnumerable<TransactionRepsonse> GetTransactions();

        public decimal GetTotalTransactionAmountByCardId(int cardId);

        public Task<(int, IEnumerable<TransactionRepsonse>)> GetTransactionPaged(TransactionParams tparams);
    }
}
