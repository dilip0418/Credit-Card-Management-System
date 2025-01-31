﻿using CCMS3.Controllers;
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
    public class CreditCardApplicationRepositoryImpl : ICreditCardApplicationRepository
    {

        private readonly AppDbContext _context;
        private readonly UserService _userService;
        public CreditCardApplicationRepositoryImpl(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public CreditCardApplicationResponse CreateCreditCardApplication(CreditCardApplicationRequest application, string applicantId)
        {
            var transaction = _context.Database.BeginTransaction();

            var personalDetails = _context.PersonalDetails.Include(u => u.User).FirstOrDefault(u => u.UserId.Equals(applicantId)) ?? throw new EntityNotFoundException("Complete your profile to apply for a credit card");
            var applicationStatus = _context.ApplicationStatuses.Find(2) ?? throw new EntityNotFoundException("Failed to fetch Application Status"); // setting the application as "Applied".

            try
            {
                var newApplication = new CreditCardApplication
                {
                    PersonalDetails = personalDetails!,
                    AnnualIncome = application.AnnualIncome,
                    ApplicationDate = application.ApplicationDate,
                    LastModifiedDate = DateTime.Now,
                    ApplicationStatus = applicationStatus!,
                    Comments = "",
                    Email = application.Email,
                    PhoneNo = application.PhoneNo,
                };

                _context.CreditCardApplications.Add(newApplication);
                var affectedRows = _context.SaveChanges();


                if (affectedRows > 0)
                {
                    var createdApplication = _context.CreditCardApplications
                        .Include(p => p.PersonalDetails)
                        .ThenInclude(p => p.User)
                        .Include(p => p.PersonalDetails.EmploymentStatus)
                        .Include(a => a.ApplicationStatus)
                        .FirstOrDefault(p => p.PersonalDetailsId.Equals(personalDetails.UserId));

                    var response = new CreditCardApplicationResponse
                    {
                        PhoneNo = createdApplication.PhoneNo,
                        Email = createdApplication.Email,
                        AnnualIncome = createdApplication.AnnualIncome,
                        ApplicationDate = createdApplication.ApplicationDate,
                        ApplicationStatus = createdApplication.ApplicationStatus.Name,
                        FullName = createdApplication.PersonalDetails.User.FullName,
                        ApplicationStatusId = createdApplication.ApplicationStatusId,
                        EmploymentStatus = createdApplication.PersonalDetails.EmploymentStatus.Status,
                        Id = createdApplication.Id
                    };
                    transaction.Commit();
                    return response;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                transaction.Rollback();
                throw;
            }
            return null;
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

                var applications = _context.CreditCardApplications
                    .Include(p => p.PersonalDetails)
                    .ThenInclude(u => u.User)
                    .Include(p => p.PersonalDetails.EmploymentStatus)
                    .Include(s => s.ApplicationStatus)
                    .Select(ca => new CreditCardApplicationResponse
                    {
                        Id = ca.Id,
                        FullName = ca.PersonalDetails.User.FullName,
                        ApplicationDate = ca.ApplicationDate,
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

        public async Task<(int, IEnumerable<CreditCardApplicationResponse>)> GetAllApplicationsPaged(CreditCardApplicationParams Params)
        {
            try
            {
                // Querying with filtering and projecting to the required fields
                var query = _context.CreditCardApplications
                    .Include(p => p.PersonalDetails)
                    .ThenInclude(p => p.User)
                    .Include(p => p.PersonalDetails.EmploymentStatus)
                    .Include(a => a.ApplicationStatus)  // Assuming ApplicationStatus is a navigational property
                    .AsQueryable();

                // Apply filters
                if (Params.ApplicationStatusId.HasValue)
                    query = query.Where(ca => ca.ApplicationStatusId == Params.ApplicationStatusId.Value);

                if (!string.IsNullOrEmpty(Params.EmailContains))
                    query = query.Where(ca => ca.Email.Contains(Params.EmailContains));

                if (!string.IsNullOrEmpty(Params.PhoneNoContains))
                    query = query.Where(ca => ca.PhoneNo.Contains(Params.PhoneNoContains));

                if (Params.ApplicationDateBefore.HasValue)
                    query = query.Where(ca => ca.ApplicationDate < Params.ApplicationDateBefore.Value);

                if (Params.ApplicationDateAfter.HasValue)
                    query = query.Where(ca => ca.ApplicationDate > Params.ApplicationDateAfter.Value);

                if (!string.IsNullOrEmpty(Params.ApplicantFullNameContains))
                    query = query.Where(ca => (ca.PersonalDetails.User.FullName)
                        .Contains(Params.ApplicantFullNameContains));

                if (Params.MinAnnualIncome.HasValue)
                    query = query.Where(ca => ca.PersonalDetails.AnnualIncome >= Params.MinAnnualIncome.Value);

                if (Params.MaxAnnualIncome.HasValue)
                    query = query.Where(ca => ca.PersonalDetails.AnnualIncome <= Params.MaxAnnualIncome.Value);

                // Apply sorting
                query = Params.SortBy switch
                {
                    "ApplicationDate" => Params.SortDescending ? query.OrderByDescending(ca => ca.ApplicationDate) : query.OrderBy(ca => ca.ApplicationDate),
                    "AnnualIncome" => Params.SortDescending ? query.OrderByDescending(ca => ca.PersonalDetails.AnnualIncome) : query.OrderBy(ca => ca.PersonalDetails.AnnualIncome),
                    _ => Params.SortDescending ? query.OrderByDescending(ca => ca.ApplicationStatus.Name) : query.OrderBy(ca => ca.ApplicationStatus.Name),
                };

                var totalRecords = query.Count();

                // Apply pagination
                query = query
                    .Skip((Params.PageNumber - 1) * Params.PageSize)
                    .Take(Params.PageSize);

                // Project to CreditCardApplicationResponse
                var result = await query.Select(ca => new CreditCardApplicationResponse
                {
                    Id = ca.Id,
                    FullName = ca.PersonalDetails.User.FullName,
                    ApplicationDate = ca.ApplicationDate,
                    Email = ca.Email,
                    PhoneNo = ca.PhoneNo,
                    ApplicationStatusId = ca.ApplicationStatusId,
                    ApplicationStatus = ca.ApplicationStatus.Name,
                    AnnualIncome = ca.PersonalDetails.AnnualIncome,
                    EmploymentStatus = ca.PersonalDetails.EmploymentStatus.Status
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
            return _context.CreditCardApplications
                .Include(p => p.PersonalDetails)
                .ThenInclude(u => u.User)
                .Include(p => p.PersonalDetails.EmploymentStatus)
                .Include(s => s.ApplicationStatus)
                .FirstOrDefault(c => c.Id == id) ?? throw new EntityNotFoundException($"No application found with id: {id}");
        }

        public CreditCardApplication GetApplicationByUserId(string userId)
        {
            var application = _context.CreditCardApplications
                .Include(p => p.PersonalDetails)
                .ThenInclude(u => u.User)
                .Include(p => p.PersonalDetails.EmploymentStatus)
                .Include(s => s.ApplicationStatus)
                .FirstOrDefault(c => c.PersonalDetailsId.Equals(userId));
            return application!;
        }

        public CreditCardApplication UpdateApplicationStatus(ApplicationStatusUpdateRequest request)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {

                var application = _context.CreditCardApplications
                    .Include(p => p.PersonalDetails)
                    .ThenInclude(u => u.User)
                    .Include(s => s.ApplicationStatus)
                    .FirstOrDefault(a => a.Id == request.Id) ?? throw new EntityNotFoundException("Failed to fetch Application");

                var applicationStatus = _context.ApplicationStatuses.Find(request.Status) ?? throw new EntityNotFoundException("Failed to fetch Application Stautus");

                application.ApplicationStatus = applicationStatus;
                application.Comments = request.Comments;

                _context.Update(application);
                _context.SaveChanges();
                transaction.Commit();
                return application;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public CreditCardApplication UpdateCreditCardApplication(CreditCardApplicationRequest request)
        {
            var application = _context.CreditCardApplications
                .Include(p => p.PersonalDetails).ThenInclude(u => u.User).Include(s => s.ApplicationStatus)
                .FirstOrDefault(c => c.Id == request.Id) ?? throw new EntityNotFoundException($"No application found with id: {request.Id}");

            application.LastModifiedDate = DateTime.Now;
            application.ApplicationStatus = _context.ApplicationStatuses.Find(application.ApplicationStatusId) ?? throw new InvalidDataException($"Error in fetching application status");

            application.Email = request.Email;
            application.PhoneNo = request.PhoneNo;
            application.AnnualIncome = request.AnnualIncome;
            application.PersonalDetails.User.FullName = request.FullName;

            _context.Update(application);
            _context.SaveChanges();

            return application;
        }
    }
}
