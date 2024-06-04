using FluentAssertions.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AutoMapper;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.B2W.Application.Catalog;
using Yah.Hub.Marketplace.B2W.Application.Client;
using Yah.Hub.Marketplace.B2W.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Catalog;
using Yah.Hub.Marketplace.Magalu.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Client;
using Xunit;
using Yah.Hub.Marketplace.MercadoLivre.Application.Client;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.MercadoLivre.Application.Category;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Magalu.Application.Catalog.Interface;
using Yah.Hub.Marketplace.B2W.Application.Catalog.Interface;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface;
using System.Drawing;

namespace Yah.Hub.Application.Marketplace.Catalog.Tests
{
    public class ServiceCollectionFixture : IDisposable
    {
        #region Properties
        public IConfiguration Configuration { get; }
        public IServiceCollection ServiceCollection { get; }
        public IServiceProvider ServiceProvider { get; }
        #endregion

        #region [Instances] 
        public ServiceCollectionFixture()
        {
            #region [Code]

            //Instance of IConfiguration
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            this.ServiceCollection = new ServiceCollection();
            this.ServiceCollection.AddTransient<IConfiguration>(x => this.Configuration);

            //Mock for Enviroment
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.SetupGet<string>(p => p.ContentRootPath).Returns(System.Environment.CurrentDirectory);
            this.ServiceCollection.AddTransient<IHostingEnvironment>(ctor => hostingEnvironment.Object);

            //Instance Markeplace Application
            new Yah.Hub.Marketplace.Application.Startup(this.Configuration).ConfigureServices(this.ServiceCollection);

            //Instance B2W dependences
            this.ServiceCollection.AddScoped<IB2WClient, B2WClient>();
            this.ServiceCollection.AddScoped<IB2WCatalogService, B2WCatalogService>();

            //Instance Magalu dependences
            this.ServiceCollection.AddScoped<IMagaluClient, MagaluClient>();
            this.ServiceCollection.AddScoped<IMagaluCatalogService, MagaluCatalogService>();

            //Instance MercadoLivre dependences
            this.ServiceCollection.AddScoped<IMercadoLivreClient, MercadoLivreClient>();
            this.ServiceCollection.AddScoped<IMercadoLivreCatalogService, MercadoLivreCatalogService>();

            //Instance AutoMapper
            Mapper.Initialize(cfg =>
                cfg.AddProfiles(
                    typeof(Yah.Hub.Marketplace.MercadoLivre.Application.Startup),
                    typeof(Yah.Hub.Marketplace.B2W.Application.Startup),
                    typeof(Yah.Hub.Marketplace.Magalu.Application.Startup))
            );

            this.ServiceProvider = this.ServiceCollection.BuildServiceProvider();

            #endregion
        }
        #endregion

        #region [Dispose]
        public void Dispose()
        { }
        #endregion
    }
}