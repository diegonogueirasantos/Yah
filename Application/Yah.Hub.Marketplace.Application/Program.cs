using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Application.Services.AnnouncementService;
using Yah.Hub.Data.Repositories.AnnouncementRepository;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.Application.Broker;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Repositories;
using Yah.Hub.Marketplace.Application.Repositories.Announcement;
using Yah.Hub.Marketplace.Application.Validation.Validators;

namespace Yah.Hub.Marketplace.Application;

public class Startup
{
    public IConfiguration Configuration { get; }
 
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
 
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // adds http client
        services.AddHttpClient();
        // adds cross cutting application
        new Yah.Hub.Application.Startup(this.Configuration).ConfigureServices(services);

        // adds marketplace application repositories
        services.AddScoped<IAnnouncementSearchRepository, AnnouncementSearchRepository>();
        services.AddScoped<IAnnouncementSearchService, AnnouncementSearchService>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAnnouncementSearchRepository, AnnouncementSearchRepository>();

        // Validation Service
        services.AddTransient<Validation.Interface.IValidationService, Validation.ValidationService>();

        // Validators
        services.AddTransient<IBoolTypeValidator, BoolTypeValidator>();
        services.AddTransient<IIntTypeValidator, IntTypeValidator>();
        services.AddTransient<IMandatoryFieldValidator, MandatoryFieldValidator>();
        services.AddTransient<IStringTypeValidator, StringTypeValidator>();
        services.AddTransient<ILessThanValidator, LessThanValidator>();
        services.AddTransient<IGreaterThanValidator, GreaterThanValidator>();
        services.AddTransient<IDecimalTypeValidator, DecimalTypeValidator>();
        services.AddTransient<IImageListLengthValidator, ImageListLengthValidator>();
        services.AddTransient<IImageSizeValidator, ImageSizeValidator>();
        services.AddTransient<IImageListTypeValidator, ImageListTypeValidator>();
        services.AddTransient<IMaxLengthValidator, MaxLengthValidator>();

        // Validation Factory
        services.AddSingleton<Validation.Interface.IValidatorFactory, Validation.ValidatorFactory>();
    }
}