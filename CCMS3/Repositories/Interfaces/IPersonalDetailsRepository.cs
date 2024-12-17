using CCMS3.Models;
using CCMS3.Controllers;
using CCMS3.Helpers.PageFilters;

namespace CCMS3.Repositories.Interfaces
{
    public interface IPersonalDetailsRepository
    {
        public PersonalDetails CreatePersonalDetails(PersonalDetails details, string UserId);
        public PersonalDetails UpdatePersonalDetails(PersonalDetailsRequest request);
        public void DeletePersonalDetails();
        public IEnumerable<PersonalDetails> GetAllPersonalDetails();
        public Task<(int, IEnumerable<PersonalDetailsRepsonse>)> GetAllPersonalDetailsPaged(PersonalDetailsParams _params);
        public PersonalDetails? GetPersonalDetailsById(string id);
        public bool HasPersonalDetails(string userId);

        public Task<IEnumerable<UserCreditCardStatus>> GetUserCreditCardStatuses(int pageNumber, int pageSize);

        public Task<int> GetTotalPersonalDetailsCountAsync();

        public UserCreditCardStatus GetCardStatusByUserId(string userId);
    }
}
