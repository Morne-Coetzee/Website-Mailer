using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Website.Mailer.Api.Services
{
    public static class ApiKeyValidationServiceCollectionExtensions
    {
        public static void AddApiKeyValidation(this IServiceCollection services, IConfiguration configuration)
        {
            var config = new ApiKeyValidationConfiguration();
            config.ApiKey = configuration.GetValue<string>(ApiKeyValidation.ApiKeyHeaderName);
            services.AddSingleton(config);

            services.AddSingleton<ApiKeyValidation>();
            services.AddScoped<ApiKeyAuthFilter>();
            services.AddHttpContextAccessor();
        }
    }

    public class ApiKeyValidationConfiguration
    {
        public string ApiKey { get; set; }
    }

    public class ApiKeyValidation(ApiKeyValidationConfiguration configuration)
    {
        public const string ApiKeyHeaderName = "X-Auth-ApiKey";

        public bool IsValidApiKey(string apiKey)
        {
            return apiKey == configuration.ApiKey;
        }
    }

    public class ApiKeyAuthFilter(ApiKeyValidation apiKeyValidation, ILogger<ApiKeyAuthFilter> logger) : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var header = context.HttpContext.Request.Headers[ApiKeyValidation.ApiKeyHeaderName];
            if (string.IsNullOrWhiteSpace(header))
            {
                logger.LogWarning("No authentication header provided in request");
                context.Result = new BadRequestResult();
                return;
            }

            if (!apiKeyValidation.IsValidApiKey(header))
            {
                logger.LogWarning("Invalid authentication header provided in request");
                context.Result = new UnauthorizedResult();
                return;
            }

            logger.LogDebug("Authentication header valid");
        }
    }

    public class ApiKeyAuthenticationAttribute : ServiceFilterAttribute
    {
        public ApiKeyAuthenticationAttribute() : base(typeof(ApiKeyAuthFilter))
        {
        }
    }
}