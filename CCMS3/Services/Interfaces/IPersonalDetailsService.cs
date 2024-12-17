using CCMS3.Models;
using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;

namespace CCMS3.Services.Interfaces
{
    public interface IPersonalDetailsService
    {
        public PersonalDetails CreatePersonalDetails(PersonalDetailsRequest request, string CurrentUserId);
        public PersonalDetails UpdatePersonalDetails(PersonalDetailsRequest request);
        public void DeletePersonalDetails();
        public IEnumerable<PersonalDetails> GetAllPersonalDetails();
        public Task<PagedResponse<PersonalDetailsRepsonse>> GetAllPersonalDetailsPaged(PersonalDetailsParams parameters);
        public PersonalDetails GetPersonalDetailsById(string id);

        public bool HasPersonalDetails(string userId);

        public Task<PagedResponse<UserCreditCardStatus>> GetUserCreditCardStatusesAsync(int pageNumber, int pageSize);

        public UserCreditCardStatus GetUserCreditCardStatus(string userId);
    }
}
