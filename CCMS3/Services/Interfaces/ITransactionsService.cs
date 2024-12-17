using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;

namespace CCMS3.Services.Interfaces
{
    public interface ITransactionsService
    {
        public Task<TransactionRepsonse> CreateTransaction(TransactionRequest request);
        public IEnumerable<TransactionRepsonse> GetTransactions();

        public Task<PagedResponse<TransactionRepsonse>> GetPagedTransactions(TransactionParams transactionParams);
    }
}
