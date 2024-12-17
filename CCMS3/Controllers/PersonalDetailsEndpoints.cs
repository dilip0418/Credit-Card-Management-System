using CCMS3.Data;
using CCMS3.Eceptions;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Services.Implementations;
using CCMS3.Services.Interfaces;
using k8s.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.ComponentModel;

namespace CCMS3.Controllers
{

    public class UserCreditCardStatus
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Status { get; set; }
        public string ApplicationStatus { get; set; }
        public string? Comments { get; set; }
    }

    public enum CreditCardStatus
    {
        HasCreditCard,
        AppliedForCreditCard,
        NoCreditCard
    }

    public class PersonalDetailsRequest
    {
        public string? Id { get; set; }
        public DateOnly DOB { get; set; }
        public AddressRequest Address { get; set; }
        public int EmploymentStatusId { get; set; }
        public decimal AnnualIncome { get; set; }
    }

    public class PersonalDetailsRepsonse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public DateOnly DOB { get; set; }
        public AddressResponse Address { get; set; }
        public int EmploymentStatusId { get; set; }
        public string EmploymentStatus { get; set; }
        public decimal AnnualIncome { get; set; }
    }

    //public class PersonalDetailsRepsonsePaged
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public DateOnly DateOfBirth { get; set; }
    //    public AddressResponse Address { get; set; }
    //    public string EmploymentStatus { get; set; }

    //}

    public class AddressRequest
    {
        public string Street { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
    }

    public class AddressResponse
    {
        public string Street { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
    }

    public static class PersonalDetailsEndpoints
    {
        public static IEndpointRouteBuilder MapPersonalDetailsEnpoints(this IEndpointRouteBuilder app)
        {

            app.MapGet("/{id}", FindById);

            app.MapPost("/create", CreatePersonalDetails);

            app.MapGet("/", GetAllDetails);

            app.MapPost("/filter", GetAllDetailsPaged);

            app.MapPut("/update", UpdatePersonalDetails);

            app.MapGet("/empStatus", GetEmploymentStatuses);

            app.MapGet("/user-card-status", GetUserCreditCardStatus);

            app.MapGet("/card-status/{userId}", GetCardStatusByUserId);
            return app;
        }

        private static ApiResponse<PersonalDetailsRepsonse> FindById(string id, IPersonalDetailsService personalDetailsService)
        {
            try
            {
                var details = personalDetailsService.GetPersonalDetailsById(id);
                var response = new PersonalDetailsRepsonse
                {
                    Id = details.UserId,
                    FullName = details.User.FullName,
                    Address = new AddressResponse
                    {
                        Street = details.Address.Street,
                        CityId = details.Address.City.Id,
                        StateId = details.Address.State.Id,
                        City = details.Address.City.Name,
                        State = details.Address.State.Name
                    },
                    DOB = details.DateOfBirth,
                    AnnualIncome = details.AnnualIncome,
                    EmploymentStatusId = details.EmploymentStatusId,
                    EmploymentStatus = details.EmploymentStatus.Status
                };
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status200OK, response, "Successfully retrieved data!");

            }
            catch(EntityNotFoundException e)
            {
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status204NoContent, null, e.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }

        }

        private static ApiResponse<PagedResponse<PersonalDetailsRepsonse>> GetAllDetailsPaged([FromBody] PersonalDetailsParams parameters, IPersonalDetailsService personalDetailsService)
        {
            try
            {
                var details = personalDetailsService.GetAllPersonalDetailsPaged(parameters).Result;
                if (details != null)
                {
                    return new ApiResponse<PagedResponse<PersonalDetailsRepsonse>>(
                                                StatusCodes.Status200OK,
                                                details,
                                                "Data retrieved successfully",
                                                details.TotalPages);
                }
                else
                {
                    return new ApiResponse<PagedResponse<PersonalDetailsRepsonse>>(
                        StatusCodes.Status204NoContent, new PagedResponse<PersonalDetailsRepsonse>(), "No Records found!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return new ApiResponse<PagedResponse<PersonalDetailsRepsonse>>(
                        StatusCodes.Status500InternalServerError, new PagedResponse<PersonalDetailsRepsonse>(), "No Records found!");
            }
        }

        public static ApiResponse<PersonalDetailsRepsonse> CreatePersonalDetails(PersonalDetailsRequest request,
            IPersonalDetailsService personalDetailsService,
            UserService userService)
        {
            try
            {
                var currentUserId = userService.GetUserId() ?? request.Id;

                var alreadyHasPersonaldetails = personalDetailsService.HasPersonalDetails(currentUserId);
                if (alreadyHasPersonaldetails)
                {
                    return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status400BadRequest, ["User already has PersonalDetails"]);
                }

                var result = personalDetailsService.CreatePersonalDetails(request, currentUserId!);
                var data = new PersonalDetailsRepsonse
                {
                    Id = result.UserId,
                    FullName = result.User.FullName,
                    Address = new AddressResponse
                    {
                        Street = result.Address.Street,
                        City = result.Address.City.Name,
                        State = result.Address.State.Name
                    },
                    AnnualIncome = result.AnnualIncome,
                    DOB = result.DateOfBirth,
                    EmploymentStatusId = result.EmploymentStatusId,
                    EmploymentStatus = result.EmploymentStatus.Status
                };
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status201Created, data, "Data created successfully");
            }
            catch (Exception ex)
            {
                // Handle the error response
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status400BadRequest, [ex.Message]);
            }
        }


        public static ApiResponse<PersonalDetailsRepsonse> UpdatePersonalDetails([FromBody] PersonalDetailsRequest request, IPersonalDetailsService personalDetailsService)
        {
            try
            {
                var updated = personalDetailsService.UpdatePersonalDetails(request);

                if (updated == null)
                {
                    return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status400BadRequest, null, "Unable to update record");
                }
                else
                {
                    var response = new PersonalDetailsRepsonse
                    {
                        Id = updated.UserId,
                        FullName = updated.User.FullName,
                        Address = new AddressResponse
                        {
                            Street = updated.Address.Street,
                            CityId = updated.Address.CityId,
                            StateId = updated.Address.StateId,
                            City = updated.Address.City.Name,
                            State = updated.Address.State.Name
                        },
                        AnnualIncome = updated.AnnualIncome,
                        DOB = updated.DateOfBirth,
                        EmploymentStatusId = updated.EmploymentStatusId,
                        EmploymentStatus = updated.EmploymentStatus.Status
                    };
                    return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status200OK, response, "Updated successfully!");
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }

        public static ApiResponse<ICollection<PersonalDetailsRepsonse>> GetAllDetails(IPersonalDetailsService personalDetailsService)
        {
            try
            {
                var details = personalDetailsService.GetAllPersonalDetails();
                var data = new List<PersonalDetailsRepsonse>();

                foreach (var d in details)
                {
                    data.Add(new PersonalDetailsRepsonse
                    {
                        Id = d.UserId,
                        FullName = d.User.FullName,
                        Address = new AddressResponse
                        {
                            Street = d.Address.Street,
                            City = d.Address.City.Name,
                            State = d.Address.State.Name
                        },
                        AnnualIncome = d.AnnualIncome,
                        DOB = d.DateOfBirth,
                        EmploymentStatusId = d.EmploymentStatusId,
                        EmploymentStatus = d.EmploymentStatus.Status
                    });
                }
                if (data.Count > 0)
                {
                    var result = new ApiResponse<ICollection<PersonalDetailsRepsonse>>
                     (
                        StatusCodes.Status200OK,
                        data,
                        "Data retrieved successfully",
                        data.Count
                     );
                    return result;
                }
                else
                {
                    var result = new ApiResponse<ICollection<PersonalDetailsRepsonse>>
                    (
                        StatusCodes.Status204NoContent,
                        [],
                        "No Reocords in DB!",
                        0
                    );
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<ICollection<PersonalDetailsRepsonse>>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }


        public static ApiResponse<ICollection<EmploymentStatus>> GetEmploymentStatuses(AppDbContext context)
        {
            try
            {
                var statuses = context.EmploymentStatuses.ToList();
                if (statuses.Count > 0)
                {
                    return new ApiResponse<ICollection<EmploymentStatus>>(StatusCodes.Status200OK, statuses, "Successsfully retrieved", statuses.Count);
                }
                else
                {
                    return new ApiResponse<ICollection<EmploymentStatus>>(StatusCodes.Status204NoContent, []);
                }
            }
            catch (Exception e)
            {
                return new ApiResponse<ICollection<EmploymentStatus>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }


        public static async Task<ApiResponse<PagedResponse<UserCreditCardStatus>>> GetUserCreditCardStatus(
            IPersonalDetailsService personalDetailsService,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize)
        {
            try
            {
                var response = await personalDetailsService.GetUserCreditCardStatusesAsync(pageNumber, pageSize);

                response ??= new PagedResponse<UserCreditCardStatus>
                {
                    Book = [],
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0,
                    TotalRecords = 0,
                };

                return new ApiResponse<PagedResponse<UserCreditCardStatus>>(StatusCodes.Status200OK, response, "Successfully retrieved");

            }
            catch (Exception e)
            {
                return new ApiResponse<PagedResponse<UserCreditCardStatus>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        public static ApiResponse<UserCreditCardStatus> GetCardStatusByUserId(
            string userId,
            IPersonalDetailsService personalDetailsService
            )
        {
            try
            {
                var status = personalDetailsService.GetUserCreditCardStatus(userId);
                if (status != null)
                    return new ApiResponse<UserCreditCardStatus>(StatusCodes.Status200OK, status, "Success");
                else
                    return new ApiResponse<UserCreditCardStatus>(StatusCodes.Status404NotFound, null, "Failed");
            }
            catch (Exception e)
            {
                return new ApiResponse<UserCreditCardStatus>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }
    }
}
