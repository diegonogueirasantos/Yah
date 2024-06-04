using AutoMapper.Configuration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Marketplace.B2W.Application.Catalog.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Catalog.Interface;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface;
using Xunit;

namespace Yah.Hub.Application.Marketplace.Catalog.Tests.Facts
{
    public class AbstractCatalogServiceFacts : IClassFixture<ServiceCollectionFixture>
    {
        #region [Properties]
        private IConfiguration Configuration { get; }
        private IB2WCatalogService B2WCatalogService { get; }
        private IMagaluCatalogService MagaluCatalogService { get; }
        private IMercadoLivreCatalogService MLCatalogService { get; }
        private ServiceCollectionFixture Fixture { get; }
        private ISecurityService SecurityService { get; }
        #endregion

        #region [Constructor]
        public AbstractCatalogServiceFacts(ServiceCollectionFixture fixture)
        {
            this.Fixture = fixture;
            this.Configuration = fixture.ServiceProvider.GetService<IConfiguration>();
            this.SecurityService = fixture.ServiceProvider.GetService<ISecurityService>();
            this.B2WCatalogService = fixture.ServiceProvider.GetService<IB2WCatalogService>();
            this.MagaluCatalogService = fixture.ServiceProvider.GetService<IMagaluCatalogService>();
            this.MLCatalogService = fixture.ServiceProvider.GetService<IMercadoLivreCatalogService>();
        }
        #endregion

        #region [Product]
        private string jsonProduct = @"{'tenantId':'1','accountId':'1','vendorId':'2','marketplace':'B2W','data':{'skus':[{'productId':'1000','id':'100','integrationId':'100','name':'SopradorTermico','price':{'list':699.00,'retail':599.00,'salePrice':null,'salePriceFrom':null,'salePriceTo':null},'inventory':{'balance':100,'handlingDays':10},'dimension':{'height':'100','width':'100','length':'100','weight':'100'},'attributes':null,'variations':[{'id':'voltage','valueId':null,'value':'127'}],'images':[{'url':'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcScUUgYQVNBicbpuOF7wU5-xc6uH_uzl8YXJA&usqp=CAU','width':900,'height':900,'isMain':false}]}],'id':'2000','name':'SopradorTermico','integrationId':'p-2000','brand':'Bosh','warrantyTime':1,'warrantyType':'year','description':'Descri��osopradortermicoteste','attributes':null,'images':[{'url':'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcScUUgYQVNBicbpuOF7wU5-xc6uH_uzl8YXJA&usqp=CAU','width':900,'height':900,'isMain':true}],'category':{'id':1,'path':[{'parentId':0,'id':5,'name':'Ferramentas'}],'name':'Sopradores','marketplaceId':null,'parentId':10}},'correlationId':'123456789','executionStep':'Requested','isSync':true,'metadata':null,'receiveCount':0,'eventDateTime':'2023-02-06T16:48:29.000Z','serviceOperation':'Insert'}";
        #endregion

        #region [Credentials]
        private readonly string VendorId = "";
        private readonly string TenantId = "";
        private readonly string AccountId = "";
        #endregion

        #region [Facts]

        #region [B2W]
        [Fact(DisplayName = "Create Product B2W")]
        public async void IntegrateProductB2W()
        {
            #region [Code]

            var product = JsonConvert.DeserializeObject<CommandMessage<Domain.Catalog.Product>>(jsonProduct);

            var identity = await this.SecurityService.ImpersonateClaimIdentity( await this.SecurityService.IssueWorkerIdentity(), VendorId, TenantId, AccountId);

            var message = new ServiceMessage<CommandMessage<Domain.Catalog.Product>>(identity, product);

            var result = await this.B2WCatalogService.ExecuteProductCommand(message);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            #endregion
        }
        #endregion

        #region [Magalu]
        [Fact(DisplayName = "Create Product Magalu")]
        public async void IntegrateProductMagalu()
        {
            #region [Code]

            var product = JsonConvert.DeserializeObject<CommandMessage<Domain.Catalog.Product>>(jsonProduct);

            var identity = await this.SecurityService.ImpersonateClaimIdentity(await this.SecurityService.IssueWorkerIdentity(), VendorId, TenantId, AccountId);

            var message = new ServiceMessage<CommandMessage<Domain.Catalog.Product>>(identity, product);

            var result = await this.MagaluCatalogService.ExecuteProductCommand(message);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            #endregion
        }
        #endregion

        #region [ML]
        [Fact(DisplayName = "Create Announcement ML")]
        public async void IntegrateProductML()
        {
            #region [Code]

            var product = JsonConvert.DeserializeObject<CommandMessage<Domain.Catalog.Product>>(jsonProduct);

            var identity = await this.SecurityService.ImpersonateClaimIdentity(await this.SecurityService.IssueWorkerIdentity(), VendorId, TenantId, AccountId);

            var message = new ServiceMessage<CommandMessage<Domain.Catalog.Product>>(identity, product);

            var result = await this.MLCatalogService.ExecuteProductCommand(message);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            #endregion
        }
        #endregion

        #endregion
    }
}