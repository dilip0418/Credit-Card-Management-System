using CCMS3.Controllers;
using CCMS3.Helpers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services.Interfaces;
using Serilog;

namespace CCMS3.Services.Implementations
{
    public class CreditCardApplicationService : ICreditCardApplicationservice
    {
        private readonly ICreditCardApplicationRepository _creditCardApplicationRepository;
        private readonly UserService _userService;
        private readonly ICreditCardService _creditCardService;
        private readonly IEmailService _emailService;
        private readonly IPersonalDetailsService _personalDetailsService;

        public CreditCardApplicationService(
            ICreditCardApplicationRepository creditCardApplicationRepository,
            UserService userService,
            ICreditCardService creditCardService,
            IEmailService emailService,
            IPersonalDetailsService personalDetailsService)
        {
            _creditCardApplicationRepository = creditCardApplicationRepository;
            _userService = userService;
            _creditCardService = creditCardService;
            _emailService = emailService;
            _personalDetailsService = personalDetailsService;
        }

        public CreditCardApplicationResponse CreateCreditCardApplication(CreditCardApplicationRequest application)
        {
            try
            {
                var applicantId = _userService.GetUserId() ?? application.ApplicantId;
                var existingApplication = this.GetApplicationByUser(applicantId);

                if (existingApplication != null)
                {
                    if (existingApplication.ApplicationStatusId == 2 || existingApplication.ApplicationStatusId == 4)
                    {
                        throw new InvalidOperationException("User already has application in process");
                    }
                }

                var newApplication = _creditCardApplicationRepository.CreateCreditCardApplication(application, applicantId);

                if (newApplication != null)
                    return newApplication;
                else
                    throw new InvalidOperationException("Failed to apply application");
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

        public async Task<PagedResponse<CreditCardApplicationResponse>> GetAllApplicationsPaged(CreditCardApplicationParams _params)
        {
            try
            {
                var applications = await _creditCardApplicationRepository.GetAllApplicationsPaged(_params);
                if (applications.Item1 > 0)
                {
                    return new PagedResponse<CreditCardApplicationResponse>(applications.Item2, applications.Item1, _params.PageNumber, applications.Item2.Count());
                }
                else
                {
                    return new PagedResponse<CreditCardApplicationResponse>([], 0, _params.PageNumber, applications.Item1);
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
                AnnualIncome = default,
                EmploymentStatus = application.PersonalDetails.EmploymentStatus.Status,
                ApplicationDate = application.ApplicationDate
            };
        }

        public async Task<string> UpdateApplicationStatusAsync(ApplicationStatusUpdateRequest request)
        {
            // Step 1: Update application Status
            // 1. If application status is saved do nothing (just save) this feature is little unclear now

            try
            {
                var application = _creditCardApplicationRepository.UpdateApplicationStatus(request);

                if (application == null)
                {
                    return "Application Not found";
                }

                if (request.Status == 3) // Accepted
                {
                    // 2. If application status is Accepted Create a Credit using the Credentials from the PersonalDetails object and may be send a mail to the user with their card number
                    try
                    {
                        return await HandleAcceptedApplicationAsync(application);
                    }

                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                        return "Application Status Updated, Falied to send Email";
                    }
                }
                else if (request.Status == 4) // Rejected
                {
                    // 3. If the application is Rejected Then send a mail to the user with proper reason for rejection

                    return await HandleRejectedApplicaitonAsync(application);
                }
                else
                {
                    return "Unknown Error Occured!";
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }

        }

        private async Task<string> HandleAcceptedApplicationAsync(CreditCardApplication application)
        {
            try
            {
                var userDetails = _personalDetailsService.GetPersonalDetailsById(application.PersonalDetailsId);

                var creditLimitInterestRate = CreditCardGenerator.CalculateCreditLimitAndInterest(userDetails.AnnualIncome, userDetails.EmploymentStatusId);

                var creditCardRequest = new CreditCardRequest
                {
                    ExpirationDate = DateTime.Now.AddYears(5),
                    CardNumber = CreditCardGenerator.GenerateCardNumber("4", 16),
                    CVV = int.Parse(CreditCardGenerator.GenerateCVV()),
                    CardHolderName = userDetails.User.FullName,
                    Balance = 0,
                    IssuedDate = DateTime.Now,
                    PersonalDetailsId = userDetails.UserId,
                    CreditLimit = creditLimitInterestRate.CreditLimit,
                    InterestRate = creditLimitInterestRate.InterestRate
                };

                var creditCard = _creditCardService.CreateCreditCard(creditCardRequest);

                var emailPayload = new CreditCardResponse
                {
                    Balance = creditCard.Balance,
                    CardHolderName = creditCard.CardHolderName,
                    CardNumber = creditCard.CardNumber,
                    CreditLimit = creditCard.CreditLimit,
                    CVV = creditCard.CVV,
                    ExpirationDate = creditCard.ExpirationDate,
                    InterestRate = creditCard.InterestRate,
                    IssuedDate = creditCard.IssuedDate,
                };
                //Initite mail sending service
                await _emailService.SendAcceptedCreditCardStatus(emailPayload, application.Email!);

                return creditCard.CardNumber;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }


        private async Task<string> HandleRejectedApplicaitonAsync(CreditCardApplication application)
        {
            try
            {
                var userDetails = _personalDetailsService.GetPersonalDetailsById(application.PersonalDetailsId);
                var emailPayload = new RejectedCreditCardResponse
                {
                    ApplicantMail = userDetails.User.Email!,
                    FullName = userDetails.User.FullName,
                    ApplicationStatus = application.ApplicationStatus.Name,
                    reasonForRejection = application.Comments
                };

                await _emailService.SendRejectedCreditCardStatus(emailPayload);
                return application.ApplicationStatus.Name;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
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

        public CreditCardApplicationResponse GetApplicationByUser(string userId)
        {
            var application = _creditCardApplicationRepository.GetApplicationByUserId(userId);
            if (application == null)
            {
                return default;
            }
            else
            {
                var response = new CreditCardApplicationResponse
                {
                    Id = application.Id,
                    ApplicationStatusId = application.ApplicationStatusId,
                    ApplicationStatus = application.ApplicationStatus.Name,
                    AnnualIncome = application.AnnualIncome,
                    ApplicationDate = application.ApplicationDate,
                    Email = application.Email,
                    FullName = application.PersonalDetails.User.FullName,
                    PhoneNo = application.PhoneNo,
                    EmploymentStatus = application.PersonalDetails.EmploymentStatus.Status,
                };
                return response;
            }
        }
    }

    public class RejectedCreditCardResponse
    {
        public string FullName { get; set; }
        public string ApplicationStatus { get; set; }
        public string reasonForRejection { get; set; }
        public string ApplicantMail { get; set; }

        // TODO: Add a link which let's the user re-edit and apply for the credit card.
    }
}
