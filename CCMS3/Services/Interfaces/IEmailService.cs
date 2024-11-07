using CCMS3.Dtos;
using CCMS3.Helpers;

namespace CCMS3.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(Mailrequest mailrequest);
        Task SendActivationEmailAsync(ActivateAccountDto dto);

        Task SendPromotionInformationalMailAsync(string email);
    }
}
