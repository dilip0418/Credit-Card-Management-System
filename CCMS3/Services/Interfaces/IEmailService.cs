using CCMS3.Controllers;
using CCMS3.Dtos;
using CCMS3.Helpers;
using CCMS3.Services.Implementations;

namespace CCMS3.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(Mailrequest mailRequest);
        Task SendActivationEmailAsync(ActivateAccountDto dto);

        Task SendPromotionInformationalMailAsync(string email);

        Task SendAcceptedCreditCardStatus(CreditCardResponse accountDetails, string cardHolderEmail);
        Task SendRejectedCreditCardStatus(RejectedCreditCardResponse emailPayload);
    }
}
