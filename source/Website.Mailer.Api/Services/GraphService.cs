using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Attachment = Microsoft.Graph.Models.Attachment;
using FileAttachment = Microsoft.Graph.Models.FileAttachment;
using Microsoft.Graph.Models;

public static class GraphService
{
    static string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
    static IConfiguration Configuration;
    static GraphService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        Configuration = builder.Build();
    }

    public static async Task HealthCheck(ILogger logger)
    {
        try
        {
            GraphServiceClient graphClient = await GetAuthenticatedGraphClientAsync(logger);
            var currentUser = await graphClient.Me.GetAsync();
            logger.LogInformation("Graph API health check passed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during Graph API health check: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task<GraphServiceClient> GetAuthenticatedGraphClientAsync(ILogger logger)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(Configuration["GraphClientId"])
            .WithTenantId(Configuration["GraphTenantId"])
            .WithClientSecret(Configuration["GraphSecret"])
            .Build();
            AuthenticationResult authResult = await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Exception sending email (Task)}", ex.Message);
            throw;
        }
        return new GraphServiceClient(httpClient);
    }

    public static async Task SendMail(string emailAddresses, string subject, string content, string bccAddresses = null, bool isHtml = false , List<FileAttachment> attachments = null, ILogger logger = null)
    {
        try
        {
            GraphServiceClient graphClient = await GetAuthenticatedGraphClientAsync(logger);
            var addresses = emailAddresses.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var recipients = addresses.Select(address => new Recipient
            {
                EmailAddress = new EmailAddress { Address = address.Trim() }
            }).ToList();
            var bccRecipients = new List<Recipient>();
            if (bccAddresses != null)
            {
                var bccAddressesList = bccAddresses.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                bccRecipients = bccAddressesList.Select(address => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = address.Trim() }
                }).ToList();
            }
            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = isHtml ? BodyType.Html : BodyType.Text,
                    Content = content
                },
                ToRecipients = recipients,
                BccRecipients = bccRecipients,
                Attachments = new List<Attachment>()
            };
            if (attachments != null)
            {
                foreach (var fileAttachment in attachments)
                {
                    if (fileAttachment != null)
                    {
                        logger?.LogInformation("Adding attachment {AttachmentName}", fileAttachment.Name ?? "Unnamed");
                        var attachment = new FileAttachment
                        {
                            OdataType = fileAttachment.OdataType,
                            ContentType = fileAttachment.ContentType,
                            Name = fileAttachment.Name,
                            ContentBytes = fileAttachment.ContentBytes
                        };
                        message.Attachments.Add(attachment);
                    }
                    else
                    {
                        logger?.LogWarning("Attempted to add a null attachment.");
                    }
                }
            }
            var request = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
            };
            await graphClient.Users[Configuration["GraphSendersEmail"]].SendMail.PostAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Exception sending email (SendMail)}", ex.Message);
            throw;
        }
    }

    public static List<FileAttachment> ConvertToGraphAttachmentList(List<FileAttachment> attachments)
    {
        var graphAttachmentList = new List<FileAttachment>();
        if (attachments != null)
        {
            foreach (Attachment attachment in attachments)
            {
                var fileAttachment = attachment as FileAttachment;
                if (fileAttachment != null && fileAttachment.ContentBytes.Length > 0)
                {
                    var graphFileAttachment = new FileAttachment
                    {
                        Name = fileAttachment.Name,
                        ContentBytes = fileAttachment.ContentBytes,
                        ContentType = fileAttachment.ContentType
                    };
                    graphAttachmentList.Add(graphFileAttachment);
                }
            }
        }
        return graphAttachmentList;
    }
}
