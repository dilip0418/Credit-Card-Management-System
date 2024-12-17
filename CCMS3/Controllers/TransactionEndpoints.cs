using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CCMS3.Controllers
{

    public class TransactionRequest
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int TTID { get; set; }
        public int CatId { get; set; }
        public int CreditCardId { get; set; }
    }

    public class TransactionRepsonse
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Descritption { get; set; }
        public int TTID { get; set; }
        public string TTName { get; set; }
        public int CatId { get; set; }
        public string CategoryName { get; set; }
        public int CredtiCardId { get; set; }
        public string CreditCardHolderName { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    public static class TransactionEndpoints
    {
        public static IEndpointRouteBuilder MapTransactionEnpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/create", CreateTransaction);
            app.MapGet("/", GetAllTransactions);
            app.MapPost("/filter", GetPagedTransactions);
            return app;
        }



        public static async Task<ApiResponse<TransactionRepsonse>> CreateTransaction(
            ITransactionsService transactionsService,
            [FromBody] TransactionRequest request)
        {
            try
            {
                var response = await transactionsService.CreateTransaction(request);
                if (response == null)
                {
                    return new ApiResponse<TransactionRepsonse>(StatusCodes.Status204NoContent, null);
                }
                else
                {
                    return new ApiResponse<TransactionRepsonse>(StatusCodes.Status200OK, response, "Suucessfully Create Transaction!!!");
                }
            }
            catch (Exception e)
            {
                return new ApiResponse<TransactionRepsonse>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        public static ApiResponse<IEnumerable<TransactionRepsonse>> GetAllTransactions(ITransactionsService transactionsService)
        {
            try
            {
                var transactions = transactionsService.GetTransactions();
                if (transactions != null && transactions.Any())
                {
                    return new ApiResponse<IEnumerable<TransactionRepsonse>>(StatusCodes.Status200OK, transactions, "Successfully returned all Transactions!!");
                }
                else
                {
                    return new ApiResponse<IEnumerable<TransactionRepsonse>>(StatusCodes.Status204NoContent, null, "No transactions found in DB!!!");
                }
            }
            catch (Exception e)
            {
                return new ApiResponse<IEnumerable<TransactionRepsonse>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        public static ApiResponse<PagedResponse<TransactionRepsonse>> GetPagedTransactions(ITransactionsService transactionsService,
            [FromBody] TransactionParams transactionParams)
        {
            try
            {
                var transactions = transactionsService.GetPagedTransactions(transactionParams).Result;
                if (transactions != null && transactions.TotalRecords != 0)
                {
                    return new ApiResponse<PagedResponse<TransactionRepsonse>>(
                        StatusCodes.Status200OK,
                        transactions,
                        "Retrieved Transactions Successfully",
                        transactions.TotalPages
                        );
                }
                else
                {
                    return new ApiResponse<PagedResponse<TransactionRepsonse>>(
                            StatusCodes.Status204NoContent, new PagedResponse<TransactionRepsonse>() { Book = [] }, "No Records found!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return new ApiResponse<PagedResponse<TransactionRepsonse>>(
                        StatusCodes.Status500InternalServerError, new PagedResponse<TransactionRepsonse>() { Book = [] }, "No Records found!");
            }
        }
    }
}
