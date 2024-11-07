using CCMS3.Helpers;
using CCMS3.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using CCMS3.Models;
using CCMS3.Data;
using CCMS3.Dtos;
using Serilog;


namespace CCMS3.Services.Implementations
{
    public class EmailServiceImpl : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly AppDbContext _context;

        public EmailServiceImpl(IOptions<EmailSettings> options, AppDbContext context)
        {
            this._emailSettings = options.Value;
            this._context = context;
        }

        public async Task SendMailAsync(Mailrequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.Email);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = mailRequest.Body
            };

            // Process attachments if there are any
            if (mailRequest.FileAttachments != null && mailRequest.FileAttachments.Count > 0)
            {
                foreach (var attachment in mailRequest.FileAttachments)
                {
                    builder.Attachments.Add(attachment.FileName, attachment.FileData, ContentType.Parse("application/octet-stream"));
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendActivationEmailAsync(ActivateAccountDto model)
        {

            // TODO: Implement error case
            //var user = _context.Users.FirstOrDefault(u => u.Email == email);
            //if (user == null)
            //{
            //}
            string activationUrl = $"http://localhost:5135/api/auth/activate-user?email={Uri.EscapeDataString(model.Email)}&code={model.ActivationCode}";
            var body = $"<p>Please activate your account by clicking the link below:</p>" +
                       $"<p><a href=\"{activationUrl}\">Activate Account</a></p>" +
                       $"<p>This link expires in 15 minutes.</p>";

            var mailRequest = new Mailrequest
            {
                ToEmail = model.Email,
                Subject = "Activate your account",
                Body = body
            };

            await SendMailAsync(mailRequest);
        }

        public async Task SendPromotionInformationalMailAsync(string email)
        {

            var body = "Hello";
            var file = new FileAttachment
            {
                FileName = "Benefits.pdf",
                FileData = await File.ReadAllBytesAsync(@"D:\\Synergech_Assessments\\MS_CSharp_Dotnet\\repos\\CCMS3\\CCMS3\\Helpers\\Promotional.pdf"),
            };
            var mailRequest = new Mailrequest
            {
                ToEmail = email,
                Subject = "Enjoy the benefits of having a credit card!",
                Body = body,
                FileAttachments = [file]
            }; try
            {
                await SendMailAsync(mailRequest);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }
    }
}
