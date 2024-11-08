using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Interfaces;
using CCMS3.Controllers;
using Serilog;
using CCMS3.Data;
using CCMS3.Helpers.PageFilters;
using Microsoft.IdentityModel.Tokens;

namespace CCMS3.Services.Implementations
{
    public class PersonalDetailsServiceImpl : IPersonalDetailsService
    {
        private readonly IPersonalDetailsRepository _personalDetailsRepository;
        private readonly AppDbContext _context;
        public PersonalDetailsServiceImpl(IPersonalDetailsRepository personalDetailsRepository, AppDbContext context)
        {
            _personalDetailsRepository = personalDetailsRepository;
            _context = context;
        }

        public PersonalDetails CreatePersonalDetails(PersonalDetailsRequest request, string UserId)
        {
            try
            {
                var State = _context.States.Find(request.Address.StateId)!;
                var City = _context.Cities.Find(request.Address.CityId)!;
                var Address = new Address
                {
                    Street = request.Address.Street,
                    State = State,
                    City = City
                };
                var details = new PersonalDetails
                {
                    DateOfBirth = request.DOB,
                    Address = Address,
                    EmploymentStatus = _context.EmploymentStatuses.Find(request.EmploymentStatusId)!
                };
                var result = _personalDetailsRepository.CreatePersonalDetails(details, UserId);
                if (result == null)
                {
                    return default;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public void DeletePersonalDetails()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PersonalDetails> GetAllPersonalDetails()
        {
            try
            {
                var result = _personalDetailsRepository.GetAllPersonalDetails();
                if (result.Any())
                {
                    return result;
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public async Task<PagedResponse<PersonalDetailsRepsonse>> GetAllPersonalDetailsPaged(PersonalDetailsParams parameters)
        {
            try
            {
                var (totalRecords, details) = await _personalDetailsRepository.GetAllPersonalDetailsPaged(parameters);
                if (totalRecords > 0)
                {
                    return new PagedResponse<PersonalDetailsRepsonse>(details, totalRecords, parameters.PageNumber, details.Count());
                }
                else
                {
                    return new PagedResponse<PersonalDetailsRepsonse>([], 0, parameters.PageNumber, totalRecords);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Something went wrong!", ex);
                throw;
            }
        }

        public PersonalDetails GetPersonalDetailsById(string id)
        {
            if (!(!id.IsNullOrEmpty()))
                throw new InvalidDataException("Id Should be an integer");
            return _personalDetailsRepository.GetPersonalDetailsById(id);

        }

        public bool HasPersonalDetails(string userId)
        {
            try
            {
                var ifExists = _personalDetailsRepository.HasPersonalDetails(userId);
                return ifExists;
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }

        public PersonalDetails UpdatePersonalDetails(PersonalDetailsRequest request)
        {
            return _personalDetailsRepository.UpdatePersonalDetails(request);
        }
    }
}
