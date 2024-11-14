using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;

namespace CCMS3.Repositories.Interfaces
{
    public interface ICreditCardApplicationRepository
    {
        public CreditCardApplicationResponse CreateCreditCardApplication(CreditCardApplicationRequest application, string applicantId);
        public CreditCardApplication UpdateCreditCardApplication(CreditCardApplicationRequest request);
        public void DeleteCreditCardApplication(int id);
        public IEnumerable<CreditCardApplicationResponse> GetAllApplications();
        public Task<(int, IEnumerable<CreditCardApplicationResponse>)> GetAllApplicationsPaged(CreditCardApplicationParams Params);
        public CreditCardApplication? GetApplicationById(int id);

        public CreditCardApplication UpdateApplicationStatus(ApplicationStatusUpdateRequest request);
    }
}
