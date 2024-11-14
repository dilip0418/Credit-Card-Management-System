using CCMS3.Helpers.Validators;
using CCMS3.Models;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Controllers
{


    public class CreditCardResponse
    {
        public int Id { get; set; }

        [CreditCardNumber(ErrorMessage = "Invalid Credit Card Number")]
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public DateTime IssuedDate { get; set; }

        [FutureDate(ErrorMessage = "Expiry Date should be a future date")]
        public DateTime ExpirationDate { get; set; }
        public int CVV { get; set; }

        [Precision(18, 2)]
        public decimal CreditLimit { get; set; }
        [Precision(18, 2)]
        public decimal Balance { get; set; }
        [Precision(18, 2)]
        public decimal InterestRate { get; set; } = 0.2M; // defaulting to 20%
    }

    public class CreditCardRequest
    {
        public int Id { get; set; }

        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int CVV { get; set; }

        [Precision(2)]
        public decimal CreditLimit { get; set; }
        [Precision(2)]
        public decimal Balance { get; set; }
        [Precision(2)]
        public decimal InterestRate { get; set; } = 0.2M; // defaulting to 20%

        public string PersonalDetailsId { get; set; }
    }
    public static class CreditcardEnpoints
    {
        public static IEndpointRouteBuilder MapCreditCardEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", GetAllCards);
            app.MapGet("/{id}", GetCreditCardById);
            app.MapDelete("/{id}", DeleteCreditCardById);
            return app;
        }

        [Authorize(Roles = "Admin")]
        public static ApiResponse<IEnumerable<CreditCardResponse>> GetAllCards(
            ICreditCardService creditCardService
            )
        {
            try
            {
                var cards = creditCardService.GetAllCreditCards();
                if (cards == null || !cards.Any())
                {
                    return new ApiResponse<IEnumerable<CreditCardResponse>>(StatusCodes.Status204NoContent,
                        [], "No Cards found!!!");
                }
                else
                {
                    return new ApiResponse<IEnumerable<CreditCardResponse>>(StatusCodes.Status200OK, cards, numOfRecords: cards.Count());
                }
            }
            catch (Exception e)
            {
                return new ApiResponse<IEnumerable<CreditCardResponse>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        [Authorize]
        public static ApiResponse<CreditCardResponse> GetCreditCardById(ICreditCardService creditCardService,
            [FromQuery] int id)
        {
            try
            {
                var card = creditCardService.GetCreditCard(id);

                if (card == null)
                    return new ApiResponse<CreditCardResponse>(
                    StatusCodes.Status204NoContent, ["Credit card not found, Apply for one!"]);

                else
                    return new ApiResponse<CreditCardResponse>(StatusCodes.Status200OK, card);
            }
            catch (Exception e)
            {
                return new ApiResponse<CreditCardResponse>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        [Authorize]
        public static ApiResponse<bool> DeleteCreditCardById(
            ICreditCardService creditCardService,
            [FromQuery] int id)
        {
            try
            {
                creditCardService.DeleteCreditCard(id);
                return new ApiResponse<bool>(StatusCodes.Status200OK, true, "Successfully deleted");
            }
            catch (Exception e)
            {
                return new ApiResponse<bool>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }


    }
}
