using CCMS3.Data;
using CCMS3.Models;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Services.Implementations
{
    public class SpendAnalysisService
    {
        private readonly AppDbContext _dbContext;

        public SpendAnalysisService(AppDbContext context)
        {
            _dbContext = context;
        }

        public async Task UpdateSpendAnalysisAsync(Transaction transaction)
        {
            var year = transaction.TransactionDate.Year;
            var month = transaction.TransactionDate.Month;

            // Check if an entry exists
            var spendAnalysis = await _dbContext.SpendAnalyses
                .FirstOrDefaultAsync(sa => sa.PersonalDetailsId == transaction.CreditCard.PersonalDetailsId &&
                                           sa.CategoryId == transaction.CategoryId &&
                                           sa.Year == year &&
                                           sa.Month == month);

            if (spendAnalysis != null)
            {
                // Update existing record
                spendAnalysis.TotalSpend += transaction.Amount;
                _dbContext.SpendAnalyses.Update(spendAnalysis);
            }
            else
            {
                // Create new record
                spendAnalysis = new SpendAnalysis
                {
                    PersonalDetailsId = transaction.CreditCard.PersonalDetailsId,
                    CategoryId = transaction.CategoryId,
                    Year = year,
                    Month = month,
                    TotalSpend = transaction.Amount,
                    Category = transaction.Category,
                    PersonalDetails = transaction.CreditCard.PersonalDetails
                };
                await _dbContext.SpendAnalyses.AddAsync(spendAnalysis);
            }

            await _dbContext.SaveChangesAsync();
        }

    }
}
