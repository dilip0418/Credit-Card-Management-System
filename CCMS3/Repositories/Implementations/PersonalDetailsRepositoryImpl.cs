using CCMS3.Controllers;
using CCMS3.Data;
using CCMS3.Eceptions;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CCMS3.Repositories.Implementations
{
    public class PersonalDetailsRepositoryImpl : IPersonalDetailsRepository
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        public PersonalDetailsRepositoryImpl(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public PersonalDetails CreatePersonalDetails(PersonalDetails details, string UserId)
        {
            try
            {
                var user = _context.Users.Find(UserId);
                details.User = user;
                if (user != null)
                {
                    _context.PersonalDetails.Add(details);
                    user.PersonalDetails = details;
                    _context.SaveChanges();
                }
                else
                {
                    return default;
                }
                return details;
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
                IList<PersonalDetails> details = [.. _context.PersonalDetails.Include(u => u.User).Include(a => a.Address).ThenInclude(c => c.City).ThenInclude(s => s.State).Include(es => es.EmploymentStatus).AsSplitQuery()];
                return details;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public async Task<(int, IEnumerable<PersonalDetailsRepsonse>)> GetAllPersonalDetailsPaged(PersonalDetailsParams _params)
        {
            try
            {
                // Querying with filtering and projecting to the required fields
                var query = _context.PersonalDetails
                    .Include(u => u.User)
                    .Include(a => a.Address).ThenInclude(c => c.City).ThenInclude(s => s.State)
                    .Include(es => es.EmploymentStatus)
                    .AsQueryable();

                // Apply filters (similar to what was discussed before)
                if (!string.IsNullOrEmpty(_params.FullNameContains))
                    query = query.Where(p => p.User.FullName.StartsWith(_params.FullNameContains));

                if (!string.IsNullOrEmpty(_params.AddressContains))
                    query = query.Where(p => p.Address.Street.Contains(_params.AddressContains));

                if (!string.IsNullOrEmpty(_params.City))
                    query = query.Where(p => p.Address.City.Name == _params.City);

                if (!string.IsNullOrEmpty(_params.State))
                    query = query.Where(p => p.Address.State.Name == _params.State);

                if (_params.DateOfBirthBefore.HasValue)
                    query = query.Where(p => p.DateOfBirth < _params.DateOfBirthBefore.Value);

                if (_params.DateOfBirthAfter.HasValue)
                    query = query.Where(p => p.DateOfBirth > _params.DateOfBirthAfter.Value);

                if (!string.IsNullOrEmpty(_params.EmploymentStatus))
                    query = query.Where(p => p.EmploymentStatus.Status == _params.EmploymentStatus);

                if (_params.MinAnnualIncome.HasValue)
                    query = query.Where(p => p.AnnualIncome >= _params.MinAnnualIncome.Value);

                if (_params.MaxAnnualIncome.HasValue)
                    query = query.Where(p => p.AnnualIncome <= _params.MaxAnnualIncome.Value);

                // Apply sorting
                query = _params.SortBy switch
                {
                    "DateOfBirth" => _params.SortDescending ? query.OrderByDescending(p => p.DateOfBirth) : query.OrderBy(p => p.DateOfBirth),
                    _ => _params.SortDescending ? query.OrderByDescending(p => p.EmploymentStatus.Status) : query.OrderBy(p => p.EmploymentStatus.Status), 
                    //"AnnualIncome" => _params.SortDescending ? query.OrderByDescending(p => p.AnnualIncome) : query.OrderBy(p => p.AnnualIncome),
                    //_ => query.OrderBy(p => p.User.FirstName) // Default sorting
                };

                var TotalRecords = await query.CountAsync();

                // Apply pagination
                query = query
                .Skip((_params.PageNumber - 1) * _params.PageSize)
                    .Take(_params.PageSize);

                // Project to PersonalDetailsResponse
                var result = await query.Select(d => new PersonalDetailsRepsonse
                {
                    //UserId = d.UserId,
                    //FullName = $"{d.User.FirstName} {d.User.LastName}",
                    DOB = d.DateOfBirth,
                    Address = new AddressResponse
                    {
                        Street = d.Address.Street,
                        City = d.Address.City.Name,
                        State = d.Address.State.Name
                    },
                    //AnnualIncome = d.AnnualIncome,
                    EmploymentStatus = d.EmploymentStatus.Status
                }).ToListAsync();

                return (TotalRecords, result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public PersonalDetails? GetPersonalDetailsById(string id)
        {
            var personalDetails = _context.PersonalDetails
                                            .Include(u => u.User)
                                            .Include(a => a.Address)
                                            .ThenInclude(a => a.City).ThenInclude(a => a.State)
                                            .Include(es => es.EmploymentStatus)
                                            .AsSplitQuery()
                                            .FirstOrDefault(p => p.UserId.Equals(id)) ?? throw new EntityNotFoundException($"Details Not found for id {id}");
            if (personalDetails != null) return personalDetails;
            return default;
        }

        public bool HasPersonalDetails(string userId)
        {
            try
            {
                var userHasPersonalDetails = _context.PersonalDetails.Any(p => p.UserId == userId);
                return userHasPersonalDetails;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public PersonalDetails UpdatePersonalDetails(PersonalDetailsRequest request)
        {
            try
            {
                
                var personalDetails = _context.PersonalDetails
                    .Include(u => u.User)
                    .Include(a => a.Address)
                    .ThenInclude(a => a.City).ThenInclude(a => a.State)
                    .Include(es => es.EmploymentStatus).AsSplitQuery()
                    .FirstOrDefault(p => p.UserId.Equals(request.Id)) ?? throw new EntityNotFoundException($"Details Not found for id {request.Id}");

                var city = _context.Cities.FirstOrDefault(c => c.Id == request.Address.CityId);
                var state = _context.States.FirstOrDefault(s => s.Id == request.Address.StateId);

                var address = new Address
                {
                    Street = request.Address.Street,
                    City = city,
                    State = state
                };
                personalDetails.Address = address;
                personalDetails.DateOfBirth = request.DOB;
                personalDetails.EmploymentStatus = _context.EmploymentStatuses.Find(request.EmploymentStatusId);
                personalDetails.AnnualIncome = request.AnnualIncome;

                var updatedResult = _context.Update(personalDetails).Entity;
                _context.SaveChanges();

                return updatedResult;
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }
    }
}
