using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Services.Implementations;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CCMS3.Controllers
{

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
        public string FullName { get; set; }
        public DateOnly DOB { get; set; }
        public AddressResponse Address { get; set; }
        public string EmploymentStatus { get; set; }
        public decimal AnnualIncomme { get; set; }
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
        public string City { get; set; }
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
            return app;
        }

        private static ApiResponse<PersonalDetailsRepsonse> FindById([FromQuery] string id, IPersonalDetailsService personalDetailsService)
        {
            try
            {
                var details = personalDetailsService.GetPersonalDetailsById(id);
                if (details == null)
                {
                    return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status204NoContent, null, $"No record found with {id}");
                }
                else
                {
                    var response = new PersonalDetailsRepsonse
                    {
                        FullName = details.User.FullName,
                        Address = new AddressResponse
                        {
                            Street = details.Address.Street,
                            City = details.Address.City.Name,
                            State = details.Address.State.Name
                        },
                        DOB = details.DateOfBirth,
                        AnnualIncomme = details.AnnualIncome,
                        EmploymentStatus = details.EmploymentStatus.Status
                    };
                    return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status200OK, response, "Successfully retrieved data!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }

        }

        private static ApiResponse<PagedResponse<PersonalDetailsRepsonse>> GetAllDetailsPaged( [FromBody] PersonalDetailsParams parameters, IPersonalDetailsService personalDetailsService)
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
                                                details.TotalRecords);
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

        public static ApiResponse<PersonalDetailsRepsonse> CreatePersonalDetails(PersonalDetailsRequest request, IPersonalDetailsService personalDetailsService, UserService userService)
        {
            try
            {
                var currentUserId = userService.GetUserId();

                var alreadyHasPersonaldetails = personalDetailsService.HasPersonalDetails(currentUserId);
                if (alreadyHasPersonaldetails)
                {
                    return new ApiResponse<PersonalDetailsRepsonse>(StatusCodes.Status400BadRequest, ["User already has PersonalDetails"]);
                }

                var result = personalDetailsService.CreatePersonalDetails(request, currentUserId!);
                var data = new PersonalDetailsRepsonse
                {
                    FullName = result.User.FullName,
                    Address = new AddressResponse
                    {
                        Street = result.Address.Street,
                        City = result.Address.City.Name,
                        State = result.Address.State.Name
                    },
                    DOB = result.DateOfBirth,
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
                        FullName = updated.User.FullName,
                        Address = new AddressResponse
                        {
                            Street = updated.Address.Street,
                            City = updated.Address.City.Name,
                            State = updated.Address.State.Name
                        },
                        DOB = updated.DateOfBirth,
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
                        FullName = d.User.FullName,
                        Address = new AddressResponse
                        {
                            Street = d.Address.Street,
                            City = d.Address.City.Name,
                            State = d.Address.State.Name
                        },
                        DOB = d.DateOfBirth,
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

    }
}
