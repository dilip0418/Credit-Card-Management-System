using CCMS3.Controllers;
using CCMS3.Data;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Implementations;
using iText.Barcodes.Dmcode;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace CCMS3.Repositories.Implementations
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly AppDbContext _context;
        private readonly SpendAnalysisService _spendAnalysisService;
        public TransactionsRepository(AppDbContext context, SpendAnalysisService saService)
        {
            _context = context;
            _spendAnalysisService = saService;
        }
        public async Task<Transaction> CreateTransactionAsync(TransactionRequest request)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var category = _context.Categories.Find(request.CatId);

                var transactionType = _context.TransactionTypes.Find(request.TTID);

                var creditCard = _context.CreditCards.
                    Include(p => p.PersonalDetails)
                    .ThenInclude(u => u.User)
                    .FirstOrDefault(c => c.Id == request.CreditCardId);

                var _transaction = new Transaction
                {
                    Amount = request.Amount,
                    Category = category,
                    CreditCard = creditCard,
                    TransactionDate = DateTime.Now,
                    Description = request.Description,
                    TransactionType = transactionType
                };

                _context.Transactions.Add(_transaction);
                var response = await _context.SaveChangesAsync();

                if(response > 0)
                {
                    await transaction.CommitAsync();
                    // Calling the spendAnalysis service method to populate the data
                    await _spendAnalysisService.UpdateSpendAnalysisAsync(_transaction);
                    return _transaction;
                }
                else
                {
                    throw new InvalidOperationException("Failed to create a Transaction");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public decimal GetTotalTransactionAmountByCardId(int cardId)
        {
            return _context.Transactions
                .Where(p => p.CreditCardId == cardId)
                .Sum(t => t.Amount);
        }

        public async Task<(int, IEnumerable<TransactionRepsonse>)> GetTransactionPaged(TransactionParams tparams)
        {
            var query = _context.Transactions
                .Include(c => c.CreditCard).ThenInclude(p => p.PersonalDetails).ThenInclude(u => u.User)
                .AsQueryable();

            if (!tparams.cardOwnerId.IsNullOrEmpty())
                query = query.Where(p => p.CreditCard.PersonalDetailsId.Equals(tparams.cardOwnerId));

            if(tparams.AmountGreaterThan >  0)
                query = query.Where(a => a.Amount > tparams.AmountGreaterThan);

            if(tparams.AmountLessThan > 0)
                query = query.Where(a => a.Amount <  tparams.AmountLessThan);

            if (tparams.DateBefore.HasValue)
                query = query.Where(d => DateOnly.FromDateTime(d.TransactionDate.Date) <= tparams.DateBefore);

            if(tparams.DateAfter.HasValue)
                query = query.Where(d => DateOnly.FromDateTime(d.TransactionDate) >= tparams.DateAfter);

            if(tparams.CategoryId > 0)
                query = query.Where(c => c.CategoryId ==  tparams.CategoryId);

            if(tparams.TypeId > 0)
                query = query.Where(t => t.TransactionTypeId == tparams.TypeId);

            if(tparams.CreditCardId > 0)
                query = query.Where(cr => cr.CreditCardId == tparams.CreditCardId);

            query = tparams.SortBy switch
            {
                "TransactionDate" => tparams.SortDescending ? query.OrderByDescending(d => d.TransactionDate) : query.OrderBy(d => d.TransactionDate),
                "Amount" => tparams.SortDescending ? query.OrderByDescending(a => a.Amount) : query.OrderBy(a => a.Amount),
                _ => query.OrderBy(t => t.TransactionId)
            };
                
            var totalRecords = await query.CountAsync();
            query = query
            .Skip((tparams.PageNumber - 1) * tparams.PageSize)
                    .Take(tparams.PageSize);

            var result = await query.Select(r => new TransactionRepsonse
            {
                TransactionId = r.TransactionId,
                Amount = r.Amount,
                CatId = r.CategoryId,
                TTID = r.TransactionTypeId,
                CategoryName = r.Category.CategoryName,
                CreditCardHolderName = r.CreditCard.PersonalDetails.User.FullName,
                CredtiCardId = r.CreditCardId,
                Descritption = r.Description,
                TTName = r.TransactionType.Type,
                TransactionDate = r.TransactionDate
            }).ToListAsync();

            return (totalRecords, result);
        }

        public IEnumerable<TransactionRepsonse> GetTransactions()
        {
            var transactions = _context.Transactions
                .Include(t => t.TransactionType)
                .Include(c => c.Category)
                .Include(cr => cr.CreditCard)
                .ThenInclude(p => p.PersonalDetails)
                .ThenInclude(u => u.User)
                .ToList();

            if (transactions.Count != 0)
            {
                return transactions.AsEnumerable().Select(r => new TransactionRepsonse
                {
                    TransactionId = r.TransactionId,
                    Amount = r.Amount,
                    CategoryName = r.Category.CategoryName,
                    CatId = r.CategoryId,
                    CreditCardHolderName = r.CreditCard.PersonalDetails.User.FullName,
                    CredtiCardId = r.CreditCardId,
                    Descritption = r.Description,
                    TTID = r.TransactionTypeId,
                    TTName = r.TransactionType.Type
                });
            }
            else
            {
                return [];
            }
        }
    }
}
