using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Interfaces;
using Serilog;
using System.Collections.Generic;

namespace CCMS3.Services.Implementations
{
    public class CreditCardApplicationService : ICreditCardApplicationservice
    {
        private readonly ICreditCardApplicationRepository _creditCardApplicationRepository;
        private readonly UserService _userService;

        public CreditCardApplicationService(ICreditCardApplicationRepository creditCardApplicationRepository, UserService userService)
        {
            _creditCardApplicationRepository = creditCardApplicationRepository;
            _userService = userService;
        }
        public CreditCardApplicationResponse CreateCreditCardApplication(CreditCardApplicationRequest application)
        {
            try
            {
                var applicantId = _userService.GetUserId();

                var newApplication = _creditCardApplicationRepository.CreateCreditCardApplication(application, applicantId);

                return newApplication;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public bool DeleteCreditCardApplication(int id)
        {
            try
            {
                _creditCardApplicationRepository.DeleteCreditCardApplication(id);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public IEnumerable<CreditCardApplicationResponse> GetAllApplications()
        {
            return _creditCardApplicationRepository.GetAllApplications();
        }

        public async Task<(int, IEnumerable<CreditCardApplicationResponse>)> GetAllApplicationsPaged(CreditCardApplicationParams _params)
        {
            try
            {
                var applications = await _creditCardApplicationRepository.GetAllApplicationsPaged(_params);
                if (applications.Item1 > 0)
                {
                    return applications;
                }
                else
                {
                    return (0, new List<CreditCardApplicationResponse>());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public CreditCardApplicationResponse? GetApplicationById(int id)
        {
            var application = _creditCardApplicationRepository.GetApplicationById(id);
            return new CreditCardApplicationResponse
            {
                Id = application.Id,
                FullName = application.PersonalDetails.User.FullName,
                ApplicationStatusId = application.ApplicationStatusId,
                ApplicationStatus = application.ApplicationStatus.Name,
                Email = application.Email,
                PhoneNo = application.PhoneNo,
                AnnualIncome = default
            };
        }

        public CreditCardApplicationResponse UpdateCreditCardApplication(CreditCardApplicationRequest application)
        {

            try
            {
                var updatedApplication = _creditCardApplicationRepository.UpdateCreditCardApplication(application);

                return new CreditCardApplicationResponse
                {
                    Id = updatedApplication.Id,
                    FullName = updatedApplication.PersonalDetails.User.FullName,
                    ApplicationStatusId = updatedApplication.ApplicationStatusId,
                    ApplicationStatus = updatedApplication.ApplicationStatus.Name,
                    Email = updatedApplication.Email,
                    PhoneNo = updatedApplication.PhoneNo,
                    AnnualIncome = default
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}
