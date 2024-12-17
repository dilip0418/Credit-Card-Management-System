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
using CCMS3.Controllers;
using iText.Forms.Form.Element;
using iText.Layout.Borders;
using iText.Layout.Properties;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using static iText.Layout.Borders.Border;


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
            email.Sender = MailboxAddress.Parse(mailRequest.FromEmail ?? _emailSettings.Email);
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

           string activationUrl = $"http://localhost:5135/api/auth/activate-user?email={Uri.EscapeDataString(model.Email)}&code={model.ActivationCode}";
            var body = $"<p>Please activate your account by clicking the link below:</p>" +
                       $"<p><a href=\"{activationUrl}\" > Activate Account </ a ></ p > " +
                       $"<button style=\"background:red; cursor:pointer\">Login</button>" ;

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

        public async Task SendAcceptedCreditCardStatus(CreditCardResponse accountDetails, string cardHolderEmail)
        {
            var body = $"<p style=\"font - size:16px; font - family:Calibri; \">Dear {accountDetails.CardHolderName},</p>" +
            $"<p style=\"font - size:14px; font - family:Calibri; \" > We are pleased to provide you with the details of your newly created credit card account:</ p > " +
$"<table style=\"border - collapse:collapse; width: 100 %; border: 1px solid #ddd;\" > " +
$"<tr style=\"background - color:#f0f0f0;\"><td style=\"padding:8px; border-bottom:1px solid #ddd; font-weight:bold;\">Card Number:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">{accountDetails.CardNumber}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">Cardholder Name:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">{accountDetails.CardHolderName}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">Issue Date:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">{accountDetails.IssuedDate}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">Expiration Date:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">{accountDetails.ExpirationDate}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">CVV:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">{accountDetails.CVV}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">Credit Limit:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">₹{accountDetails.CreditLimit}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">Current Balance:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">₹{accountDetails.Balance}</td></tr>" +
$"<tr><td style=\"padding: 8px; border - bottom:1px solid #ddd; font-weight:bold;\">Interest Rate:</td><td style=\"padding:8px; border-bottom:1px solid #ddd;\">{accountDetails.InterestRate}%</td></tr>" +
$"</table>" +
            $"<p style=\"font - size:14px; font - family:Calibri; \">If you have any questions or concerns, please don't hesitate to contact us.</p>" +
            $"<p style=\"font - size:14px; font - family:Calibri; \">Thank you for choosing our services.</p>" +
            $"<p style=\"font - size:14px; font - family:Calibri; \">Best regards,</p>" +
$"<p style=\"font - size:14px; font - family:Calibri; \">easycreds.support@gmail.com</p>";

            var mailRequest = new Mailrequest
            {
                ToEmail = cardHolderEmail,
                Subject = "Congratulations! you've got yourself a brand new Credit Card 🎉",
                Body = body
            };

            try
            {
                await SendMailAsync(mailRequest);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }

        }

        public async Task SendRejectedCreditCardStatus(RejectedCreditCardResponse emailPayload)
        {
            var body = $"<p style=\"font - size:16px; font - family:Calibri; \">Dear {emailPayload.FullName},</p>" +
$"<p style=\"font - size:14px; font - family:Calibri; \">We regret to inform you that your credit card application has been {emailPayload.ApplicationStatus}.</p>" +
$"<p style=\"font - size:14px; font - family:Calibri; \">Reason for rejection: {emailPayload.reasonForRejection}</p>" +
"<p style=\"font - size:14px; font - family:Calibri; \">You can look into the pdf attached below for the steps to successfully apply for a credit card.</p>" +
"<p style=\"font - size:14px; font - family:Calibri; \">We appreciate your interest in our services. For further clarification or re-application, please contact us at <a href=\"mailto: easycreds.support @gmail.com\">easycreds.support@gmail.com</a>.</p>" +
"<p style=\"font - size:14px; font - family:Calibri; \">Thank you for considering EasyCreds.</p>" +
"<p style=\"font - size:14px; font - family:Calibri; \">Best regards,</p>" +
"<p style=\"font - size:14px; font - family:Calibri; \">EasyCreds Team</p>";
            var file = new FileAttachment
            {
                FileName = "Benefits.pdf",
                FileData = await File.ReadAllBytesAsync(@"D:\\Synergech_Assessments\\MS_CSharp_Dotnet\\repos\\CCMS3\\CCMS3\\Helpers\\Promotional.pdf"),
            };
            var emailRequest = new Mailrequest
            {
                ToEmail = emailPayload.ApplicantMail,
                Subject = "Sorry! you application was rejected... 😔",
                Body = body,
                FileAttachments = [file]

            };

            try
            {
                await SendMailAsync(emailRequest);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }
    }
}
