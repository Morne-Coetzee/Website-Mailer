using HealthChecks.UI.Client;
using Microsoft.OpenApi.Models;
using Serilog;
using Website.Mailer.Api.HealthChecks;
using Website.Mailer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Logger(l => {
        var enableFileLogging = context.Configuration.GetValue<bool>("EnableFileLogging");
        if (enableFileLogging)
        {
            l.Filter.ByIncludingOnly(evt => evt.Level >= Serilog.Events.LogEventLevel.Debug)
                .WriteTo.File(
                    path: "Logs/cp_.log",
                    outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7);
        }
    })
);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = ApiKeyValidation.ApiKeyHeaderName,
        Description = "API Key Authentication",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey",
                },
            },
            new string[] { }
        }
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<EmailServiceHealthCheck>(nameof(EmailServiceHealthCheck));
builder.Services.AddEmailService(builder.Configuration);
builder.Services.AddApiKeyValidation(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks($"/_health", new()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.Run();
