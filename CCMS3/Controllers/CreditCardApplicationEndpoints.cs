using CCMS3.Models;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CCMS3.Controllers
{

    public class CreditCardApplicationRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone, MinLength(10, ErrorMessage = "Phone number must be of 10 digits"), MaxLength(10, ErrorMessage = "Phone number must be of 10 digits")]
        public string PhoneNo { get; set; }
        public int ApplicationStatusId { get; set; }
        public decimal AnnualIncome { get; set; }
    }

    public class CreditCardApplicationResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public int ApplicationStatusId { get; set; }
        public string ApplicationStatus { get; set; }
        public decimal AnnualIncome { get; set; }

    }
    public static class CreditCardApplicationEndpoints
    {
        public static IEndpointRouteBuilder MapCreditCardApplicationEnpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", GetAllApplications);

            return app;
        }

        [Authorize(Roles = "Admin")]
        public static ApiResponse<IEnumerable<CreditCardApplicationResponse>> GetAllApplications(ICreditCardApplicationservice creditCardApplicationservice)
        {
            try
            {
                var applications = creditCardApplicationservice.GetAllApplications();
                if (applications != null && applications.Any())
                {
                    return new ApiResponse<IEnumerable<CreditCardApplicationResponse>>(StatusCodes.Status200OK, applications);
                }
                else
                {
                    return new ApiResponse<IEnumerable<CreditCardApplicationResponse>>(StatusCodes.Status204NoContent, [],"No Applications found in db");
                }
            }
            catch (Exception ex)
            {

                return new ApiResponse<IEnumerable<CreditCardApplicationResponse>>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }
    }
}
