using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.AspNetCore.Mvc.Filters;
using Yah.Hub.Scheduler.AuthorizationFilters;
using Yah.Hub.Scheduler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string ports = $"http://*:{builder.Configuration.GetSection("HTTP_PORT").Value}";
if (builder.Configuration.GetSection("HTTPS_PORT").Value != null)
    ports = $"{ports};{builder.Configuration.GetSection("HTTPS_PORT").Value}";

builder.WebHost.UseUrls(ports);

new Yah.Hub.Application.Startup(builder.Configuration).ConfigureServices(builder.Services);

// Add the processing server as IHostedService
builder.Services.AddHangfire(configuration =>
{
    configuration.UseStorage(
        new MySqlStorage(
            $"server={builder.Configuration.GetSection("MySql:Uri").Value};uid={builder.Configuration.GetSection("MySql:User").Value};pwd={builder.Configuration.GetSection("MySql:Pass").Value};database={builder.Configuration.GetSection("MySql:Catalog").Value};Allow User Variables=True",
            new MySqlStorageOptions
            {
                TablesPrefix = "Hangfire"
            }
        )
    );
});

builder.Services.AddHangfireServer(options => options.WorkerCount = 25);
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<IHangfireService, HangfireService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new CustomAF() }
});
app.UseAuthorization();
app.MapControllers();
app.Run();
