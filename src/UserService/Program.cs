using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Middlewares;
using UserService.Services;
using Consul;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

string serviceName = "UserService";
string jaegerHost = builder.Configuration.GetValue<string>("Jaeger:Host") ?? "localhost";
int jaegerPort = builder.Configuration.GetValue<int>("Jaeger:Port") == 0 ? 6831 : builder.Configuration.GetValue<int>("Jaeger:Port");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(opt =>
            {
                opt.AgentHost = jaegerHost;
                opt.AgentPort = jaegerPort;
            });
    }); ;

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService.Services.UserService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. Add in header as 'X-Api-Key'",
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                Scheme = "ApiKeyScheme",
                Name = "X-Api-Key",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.WebHost.UseUrls("http://+:80");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<ErrorLoggingMiddleware>();

app.MapGet("/health", () => Results.Ok("Healthy"));

var consulClient = new ConsulClient(config =>
{
    config.Address = new Uri("http://consul:8500"); 
});

var registration = new AgentServiceRegistration
{
    ID = "userservice",
    Name = "userservice",
    Address = "userservice",
    Port = 80,
    Check = new AgentServiceCheck
    {
        HTTP = "http://userservice/health",
        Interval = TimeSpan.FromSeconds(10),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
    }
};

app.Lifetime.ApplicationStarted.Register(() =>
{
    consulClient.Agent.ServiceRegister(registration).Wait();
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
});

app.MapControllers();

app.Run();