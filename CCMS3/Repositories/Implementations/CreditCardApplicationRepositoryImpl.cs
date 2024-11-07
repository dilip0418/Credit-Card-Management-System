using CCMS3.Controllers;
using CCMS3.Data;
using CCMS3.Eceptions;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CCMS3.Repositories.Implementations
{
    public class CreditCardApplicationRepositoryImpl : ICreditCardApplicationRepository
    {

        private readonly AppDbContext _context;
        public CreditCardApplicationRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public CreditCardApplication CreateCreditCardApplication(CreditCardApplication application, string applicantId)
        {
            throw new NotImplementedException();
        }

        public void DeleteCreditCardApplication(int id)
        {
            try
            {
                var application = _context.CreditCardApplications.Find(id);
                if (application != null)
                {
                    _context.Remove(application);
                    _context.SaveChanges();
                }
                else
                {
                    throw new EntityNotFoundException($"No application found with id: {id}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public IEnumerable<CreditCardApplicationResponse> GetAllApplications()
        {
            try
            {

                var applications = _context.CreditCardApplications.Select(ca => new CreditCardApplicationResponse
                {
                    Id = ca.Id,
                    FullName = ca.PersonalDetails.User.FullName,
                    MyProperty = ca.ApplicationDate,
                    Email = ca.Email,
                    PhoneNo = ca.PhoneNo,
                    ApplicationStatusId = ca.ApplicationStatusId,
                    ApplicationStatus = ca.ApplicationStatus.Name,
                    AnnualIncome = ca.PersonalDetails.AnnualIncome
                }).ToListAsync().Result;

                if (applications.Count != 0)
                {
                    return applications;
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public async Task<(int, IEnumerable<CreditCardApplicationResponse>)> GetAllApplicationsPaged(CreditCardApplicationParams _params)
        {
            try
            {
                // Querying with filtering and projecting to the required fields
                var query = _context.CreditCardApplications
                    .Include(ca => ca.PersonalDetails)
                    .Include(ca => ca.ApplicationStatus)  // Assuming ApplicationStatus is a navigational property
                    .AsQueryable();

                // Apply filters
                if (_params.ApplicationStatusId.HasValue)
                    query = query.Where(ca => ca.ApplicationStatusId == _params.ApplicationStatusId.Value);

                if (!string.IsNullOrEmpty(_params.EmailContains))
                    query = query.Where(ca => ca.Email.Contains(_params.EmailContains));

                if (!string.IsNullOrEmpty(_params.PhoneNoContains))
                    query = query.Where(ca => ca.PhoneNo.Contains(_params.PhoneNoContains));

                if (_params.ApplicationDateBefore.HasValue)
                    query = query.Where(ca => ca.ApplicationDate < _params.ApplicationDateBefore.Value);

                if (_params.ApplicationDateAfter.HasValue)
                    query = query.Where(ca => ca.ApplicationDate > _params.ApplicationDateAfter.Value);

                if (!string.IsNullOrEmpty(_params.ApplicantFullNameContains))
                    query = query.Where(ca => (ca.PersonalDetails.User.FullName)
                        .Contains(_params.ApplicantFullNameContains));

                if (_params.MinAnnualIncome.HasValue)
                    query = query.Where(ca => ca.PersonalDetails.AnnualIncome >= _params.MinAnnualIncome.Value);

                if (_params.MaxAnnualIncome.HasValue)
                    query = query.Where(ca => ca.PersonalDetails.AnnualIncome <= _params.MaxAnnualIncome.Value);

                // Apply sorting
                query = _params.SortBy switch
                {
                    "ApplicationDate" => _params.SortDescending ? query.OrderByDescending(ca => ca.ApplicationDate) : query.OrderBy(ca => ca.ApplicationDate),
                    "AnnualIncome" => _params.SortDescending ? query.OrderByDescending(ca => ca.PersonalDetails.AnnualIncome) : query.OrderBy(ca => ca.PersonalDetails.AnnualIncome),
                    _ => _params.SortDescending ? query.OrderByDescending(ca => ca.ApplicationStatus.Name) : query.OrderBy(ca => ca.ApplicationStatus.Name),
                };

                var totalRecords = query.Count();

                // Apply pagination
                query = query
                    .Skip((_params.PageNumber - 1) * _params.PageSize)
                    .Take(_params.PageSize);

                // Project to CreditCardApplicationResponse
                var result = await query.Select(ca => new CreditCardApplicationResponse
                {
                    Id = ca.Id,
                    FullName = ca.PersonalDetails.User.FullName,
                    MyProperty = ca.ApplicationDate,
                    Email = ca.Email,
                    PhoneNo = ca.PhoneNo,
                    ApplicationStatusId = ca.ApplicationStatusId,
                    ApplicationStatus = ca.ApplicationStatus.Name,
                    AnnualIncome = ca.PersonalDetails.AnnualIncome
                }).ToListAsync();

                return (totalRecords, result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }

        }

        public CreditCardApplication? GetApplicationById(int id)
        {
            return _context.CreditCardApplications.Find(id) ?? throw new EntityNotFoundException($"No application found with id: {id}");
        }

        public CreditCardApplication UpdateCreditCardApplication(CreditCardApplicationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
