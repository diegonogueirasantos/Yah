using AutoMapper;
using Nest;
using Yah.Hub.Marketplace.Netshoes.Application;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string ports = $"http://*:{builder.Configuration.GetSection("HTTP_PORT").Value}";
if (builder.Configuration.GetSection("HTTPS_PORT").Value != null)
    ports = $"{ports};https://*:{builder.Configuration.GetSection("HTTPS_PORT").Value}";

builder.WebHost.UseUrls(ports);

//AutoMapper
builder.Services.AddAutoMapper(typeof(Yah.Hub.Marketplace.Netshoes.Application.Startup));

// call startups
new Yah.Hub.Marketplace.Netshoes.Application.Startup(builder.Configuration).ConfigureServices(builder.Services);

Yah.Hub.Common.Startup.ConfigureLogging(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), builder);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();