using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;
using CCMS3.Models;

namespace CCMS3.Services.Interfaces
{
    public interface ICreditCardApplicationservice
    {
        public CreditCardApplicationResponse CreateCreditCardApplication(CreditCardApplicationRequest application);
        public CreditCardApplicationResponse UpdateCreditCardApplication(CreditCardApplicationRequest application);
        public bool DeleteCreditCardApplication(int id);
        public IEnumerable<CreditCardApplicationResponse> GetAllApplications();
        public Task<PagedResponse<CreditCardApplicationResponse>> GetAllApplicationsPaged(CreditCardApplicationParams _params);
        public CreditCardApplicationResponse? GetApplicationById(int id);

        public CreditCardApplicationResponse GetApplicationByUser(string userId);
        public Task<string> UpdateApplicationStatusAsync(ApplicationStatusUpdateRequest request);
    }
}
