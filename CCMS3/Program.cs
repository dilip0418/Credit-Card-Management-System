using CCMS3.Controllers;
using CCMS3.Extensions;
using CCMS3.Middlewares;
using CCMS3.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuring Serilog for logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

builder.Services
    .AddSwaggerExplorer()
    .InjectDbContext(builder.Configuration)
    .AddAppConfig(builder.Configuration)
    .AddMailConfig(builder.Configuration)
    .AddIdentityHandlersAndStores()
    .AddIdentityAuth(builder.Configuration);


builder.Services.AddHttpContextAccessor();

// Register a service (communicates to an external Api)
builder.Services.AddHttpClient<StateCityService>();
    
// Inject required dependencies
builder.Services.InjectDependencies();

// configure HealthChecks
builder.Services.InjectHealthChecks(builder.Configuration);

var app = builder.Build();

app.UseHealthCheck();

await app.Services.SeedAdminUserAsync();
await app.SeedDatabaseAsync();

app.ConfigureSwaggerExplorer()
    .AddIdentityAuthMiddleWares();

app.MapControllers();

app.UseMiddleware<PdfCompressionMiddleware>();

//app.MapGroup("/api")
//    .MapIdentityApi<AppUser>();

app.MapAuthorizationDemoEndpoints();

app.MapGroup("/api/auth")
    .MapIdentityUserEndpoints();

app.MapGroup("/api/roles")
    .MapRolesEndpoints();

app.MapGroup("/api/personalDetails")
    .MapPersonalDetailsEnpoints();

app.MapGroup("/api/creditCardApplications")
    .MapCreditCardApplicationEnpoints();

await app.RunAsync();
