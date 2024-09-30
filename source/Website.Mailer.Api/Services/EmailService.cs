using Microsoft.Extensions.Options;
using Website.Mailer.Api.Models;
using FileAttachment = Microsoft.Graph.Models.FileAttachment;

namespace Website.Mailer.Api.Services
{
    public static class EmailServiceServiceCollectionExtensions
    {
        public static void AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<EmailService>();
            services.Configure<EmailServiceOptions>(configuration.GetSection(nameof(EmailService)));
        }
    }

    public class EmailService(ILogger<EmailService> logger, IOptions<EmailServiceOptions> options)
    {
        public List<FileAttachment> GraphAttachments { get; private set; }

        public async Task<Task> HealthCheck()
        {
            await GraphService.HealthCheck(logger);
            return Task.CompletedTask;
        }

        public async Task SendAsync(EmailRequestModel model)
        {
            List<string> recipients = model.To.Select(email => email.Trim()).ToList();
            logger.LogDebug("Sending email with subject {Subject} to: {Recipients}", model.Subject, MaskEmailAddress(string.Join(";", model.To)));
            if (!string.IsNullOrEmpty(options.Value.RedirectTo))
            {
                logger.LogDebug("Redirecting email to: {Recipients}", MaskEmailAddress(options.Value.RedirectTo));
                recipients = options.Value.RedirectTo.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(email => email.Trim()).ToList();
            }
            try
            {
                List<FileAttachment> graphAttachments = null;
                if (model.Attachments != null && model.Attachments.Count > 0)
                {
                    graphAttachments = new List<FileAttachment>();
                    for (int i = 0; i < model.Attachments.Count; i++)
                    {
                        var attachmentModel = model.Attachments[i];
                        var attachment = new FileAttachment
                        {
                            ContentBytes = attachmentModel.Data,
                            ContentType = attachmentModel.MimeType,
                            Name = attachmentModel.FileName
                        };
                        graphAttachments.Add(attachment);
                    }
                }
                var graphAttachmentList = graphAttachments != null ? GraphService.ConvertToGraphAttachmentList(graphAttachments) : null;
                await GraphService.SendMail(
                    string.Join(";", recipients),
                    model.Subject,
                    model.Body,
                    null,
                    true,
                    graphAttachmentList,
                    logger
                );
                logger.LogDebug("Email sent successfully");
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email: {Message}", ex.Message);
                throw;
            }
        }

        private string MaskEmailAddress(string emailAddress)
        {
            var emails = emailAddress.ToString().Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var maskedEmails = new List<string>();
            foreach (string mail in emails)
            {
                var parts = mail.Split('@');
                var username = parts[0];
                var domain = parts[1];
                if (username.Length <= 2)
                {
                    username = new string('*', username.Length);
                }
                else
                {
                    username = username[0..2] + new string('*', username.Length - 4) + username[^2..];
                }
                maskedEmails.Add($"{username}@{domain}");
            }
            return string.Join(';', maskedEmails);
        }
    }
}
