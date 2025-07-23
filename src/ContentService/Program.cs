using Consul;
using ContentService.ClientServices;
using ContentService.Data;
using ContentService.Helpers;
using ContentService.Middlewares;
using ContentService.Services;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


string serviceName = "ContentService";
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
    });


builder.Host.UseSerilog();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    var address = builder.Configuration["ConsulConfig:Address"] ?? "http://consul:8500";
    consulConfig.Address = new Uri(address);
}));

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContentService, ContentService.Services.ContentService>();

builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UserService:BaseUrl"] ?? "http://userservice:80");
})
.AddPolicyHandler(PolicyHelper.GetCombinedPolicy());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseMiddleware<ErrorLoggingMiddleware>();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapControllers();

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = $"contentservice-{Guid.NewGuid()}",
    Name = "contentservice",
    Address = "contentservice", // docker-compose'daki servis adıyla aynı olmalı
    Port = 80,
    Check = new AgentServiceCheck()
    {
        HTTP = "http://contentservice/health",
        Interval = TimeSpan.FromSeconds(10),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
    }
};

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        consulClient.Agent.ServiceRegister(registration).Wait();
        Log.Information("Service registered to Consul successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Consul service registration failed.");
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    try
    {
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        Log.Information("Service deregistered from Consul successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Consul service deregistration failed.");
    }
});

app.Run();