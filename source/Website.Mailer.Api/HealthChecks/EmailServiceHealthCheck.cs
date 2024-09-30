using Microsoft.Extensions.Diagnostics.HealthChecks;
using Website.Mailer.Api.Services;

namespace Website.Mailer.Api.HealthChecks
{
    public class EmailServiceHealthCheck(EmailService emailService) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await emailService.HealthCheck();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}
