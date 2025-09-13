using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using Serilog;
using UserApi.Data;
using UserApi.Services;
using UserApi.Infrastructure;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with enhanced observability
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "user-api";
var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
var environment = builder.Environment.EnvironmentName;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .Enrich.WithProperty("ServiceVersion", serviceVersion)
    .Enrich.WithProperty("Environment", environment)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";
        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
    })
    .CreateLogger();

builder.Host.UseSerilog();


// Add services to the container
builder.Services.AddControllers(options =>
{
    // Remove the default JSON formatter that has PipeWriter issues in .NET 9
    var defaultFormatter = options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();
    if (defaultFormatter != null)
    {
        options.OutputFormatters.Remove(defaultFormatter);
    }

    // Add our custom JSON formatter that avoids PipeWriter
    options.OutputFormatters.Add(new CompatibleSystemTextJsonOutputFormatter(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "User API", Version = "v1" });
});

// Add Entity Framework with In-Memory database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("UserDb"));

// Add services
builder.Services.AddScoped<IUserService, UserService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserDbContext>("database");

// Configure OpenTelemetry with enhanced resource attributes
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = environment,
            ["service.instance.id"] = Environment.MachineName,
            ["service.namespace"] = "otel-core-example",
            ["host.name"] = Environment.MachineName,
            ["os.type"] = Environment.OSVersion.Platform.ToString(),
            ["runtime.name"] = ".NET",
            ["runtime.version"] = Environment.Version.ToString(),
            ["container.id"] = Environment.GetEnvironmentVariable("HOSTNAME") ?? "localhost"
        }))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                };
                options.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                };
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.SetDbStatementForStoredProcedure = true;
            })
            .AddHttpClientInstrumentation()
            .AddSource("UserApi")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    });

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSerilog();
});

// Add CORS with secure configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                           ?? new[] { "http://localhost:3000", "https://localhost:3001" };
        
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("SecureCors");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Map health checks endpoints with limited information exposure
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString()
                // Removed description and duration to prevent information disclosure
            })
            // Removed totalDuration to prevent timing attack information
        });
        await context.Response.WriteAsync(result);
    }
});

// Metrics endpoint for Prometheus scraping (if available)
app.MapGet("/metrics", async (HttpContext context) =>
{
    context.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
    await context.Response.WriteAsync("# OpenTelemetry metrics endpoint\n");
    await context.Response.WriteAsync("# Metrics are exported via OpenTelemetry to Alloy\n");
});

// Additional endpoints for observability with limited information exposure
app.MapGet("/info", () => new
{
    service = serviceName,
    version = serviceVersion,
    status = "running"
    // Removed environment, timestamp, and uptime to prevent information disclosure
}).WithName("Info").WithTags("Observability");

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    context.Database.EnsureCreated();
}

try
{
    Log.Information("Starting User API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class accessible to the test project
public partial class Program { }
