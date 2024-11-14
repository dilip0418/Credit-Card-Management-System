using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public class ApplicationStatusUpdateRequest
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string Comments { get; set; }
    }

    
    public static class CreditCardApplicationEndpoints
    {
        public static IEndpointRouteBuilder MapCreditCardApplicationEnpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{id}", GetApplicationById);
            app.MapGet("/", GetAllApplications);
            app.MapPost("/filter", GetAllApplicationsPaged);
            app.MapPost("/create", CreateApplication);
            app.MapPost("/update", UpdateApplication);
            app.MapDelete("/{id}",DeleteApplicationById);
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
                    return new ApiResponse<IEnumerable<CreditCardApplicationResponse>>(StatusCodes.Status204NoContent, [], "No Applications found in db");
                }
            }
            catch (Exception ex)
            {

                return new ApiResponse<IEnumerable<CreditCardApplicationResponse>>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }

        [Authorize(Roles = "Admin")]
        public static ApiResponse<PagedResponse<CreditCardApplicationResponse>> GetAllApplicationsPaged(
            [FromBody] CreditCardApplicationParams creditCardApplicationParams,
            ICreditCardApplicationservice creditCardApplicationservice)
        {
            try
            {
                var pagedApplications = creditCardApplicationservice.GetAllApplicationsPaged(creditCardApplicationParams).Result;

                if (pagedApplications.TotalRecords > 0)
                {
                    return new ApiResponse<PagedResponse<CreditCardApplicationResponse>>(
                        StatusCodes.Status200OK,
                        pagedApplications,
                        "Retrieved applications successfully",
                        pagedApplications.TotalRecords);
                }
                else
                {
                    return new ApiResponse<PagedResponse<CreditCardApplicationResponse>>(
                        StatusCodes.Status204NoContent, new PagedResponse<CreditCardApplicationResponse>(), "No application found", 0);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponse<CreditCardApplicationResponse>>(
                    StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }

        [Authorize]
        public static ApiResponse<CreditCardApplicationResponse> CreateApplication(
            ICreditCardApplicationservice creditCardApplicationservice,
            [FromBody] CreditCardApplicationRequest applicationRequest)
        {
            try
            {
                var response = creditCardApplicationservice.CreateCreditCardApplication(applicationRequest);
                if (response == null)
                {
                    return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status500InternalServerError, ["Unknown Error Occured while creating credit card application"]);
                }
                else
                {
                    return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status200OK, response, "Successfully applied an application");
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }

        [Authorize]
        public static ApiResponse<CreditCardApplicationResponse> UpdateApplication(
            ICreditCardApplicationservice creditCardApplicationservice,
            [FromBody] CreditCardApplicationRequest applicationRequest)
        {
            try
            {
                var response = creditCardApplicationservice.UpdateCreditCardApplication(applicationRequest);
                if (response == null)
                {
                    return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status500InternalServerError, ["Unknown Error Occured while updating credit card application"]);
                }
                else
                {
                    return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status200OK, response, "Successfully applied an application");
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }


        [Authorize]
        public static ApiResponse<CreditCardApplicationResponse> GetApplicationById(
            [FromQuery] int id,
            ICreditCardApplicationservice creditCardApplicationservice)
        {
            try
            {
                return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status200OK, creditCardApplicationservice.GetApplicationById(id), "Successfully Retrieved applicaiton")
                    ?? new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status204NoContent, default, $"No application found with id {id}");
            }
            catch (Exception e)
            {
                return new ApiResponse<CreditCardApplicationResponse>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        [Authorize]
        public static ApiResponse<bool> DeleteApplicationById(
            [FromQuery] int id,
            ICreditCardApplicationservice creditCardApplicationservice)
        {
            try
            {

                return new ApiResponse<bool>(StatusCodes.Status200OK, true, $"Successfully Deleted the applciation with id: {id}") ?? new ApiResponse<bool>(StatusCodes.Status404NotFound, [$"Failed to deleted applicaiton with id: {id}"]);
            }
            catch (Exception e)
            {
                return new ApiResponse<bool>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        public static ApiResponse<string> UpdateApplicationStatus(
            [FromBody] ApplicationStatusUpdateRequest request,
            ICreditCardApplicationservice creditCardApplicationservice)
        {

            try
            {
                var response = creditCardApplicationservice.UpdateApplicationStatusAsync(request);
                if (response.Result.Equals("Rejected"))
                {
                    return new ApiResponse<string>(StatusCodes.Status200OK, response.Result, "Status Updated");
                }
                else
                {
                    return new ApiResponse<string>(StatusCodes.Status200OK, response.Result, "Status Updated");
                }
            }
            catch (Exception e)
            {
                return new ApiResponse<string>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }
    }
}
