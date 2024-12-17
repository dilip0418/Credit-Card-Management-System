using CCMS3.Helpers;
using CCMS3.Models;
using CCMS3.Services.Implementations;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CCMS3.Controllers
{
    public class MailFormModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public static class ContactEndpoints
    {
        public static IEndpointRouteBuilder MapContactEnpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/contact", SendMessageAsyncFromUser);
            app.MapPost("/api/advertise", SendMessageAsyncFromUser);
            return app;
        }

        public static async Task<ApiResponse<string>> SendMessageAsyncFromUser(
            [FromBody] MailFormModel model,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings
            )
        {
            try
            {
                var body = $"<h3>From {model.Name ?? "CCMS"},</h3>" +
                    $"<p>{model.Body}</p>";

                var mailRequest = new Mailrequest
                {
                    FromEmail = model.Email,
                    ToEmail = emailSettings.Value.Email,
                    Subject = model.Subject,
                    Body = body
                };

                await emailService.SendMailAsync(mailRequest);

                return new ApiResponse<string>(StatusCodes.Status200OK, "Message Sent Successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }

        public static async Task<ApiResponse<string>> SendMessageAsyncFromAdminToUser(
            [FromBody] MailFormModel model,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings
            )
        {
            try
            {
                var mailRequest = new Mailrequest
                {
                    FromEmail = emailSettings.Value.Email,
                    ToEmail = model.Email,
                    Subject = model.Subject,
                    Body = model.Body
                };

                await emailService.SendMailAsync(mailRequest);

                return new ApiResponse<string>(StatusCodes.Status200OK, "Mail Sent Successfull");
            }catch(Exception e)
            {
                return new ApiResponse<string>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }
    }
}
