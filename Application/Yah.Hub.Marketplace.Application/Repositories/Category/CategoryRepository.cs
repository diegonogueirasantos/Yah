using System;
using Nest;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Category;
using Yah.Hub.Common.Enums;
using StackExchange.Redis;
using Yah.Hub.Common.Services;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Domain.Monitor;

namespace Yah.Hub.Marketplace.Application.Repositories
{
    public class CategoryRepository : AbstractElasticSearchRepository<MarketplaceCategory>, ICategoryRepository
    {
        public CategoryRepository(ILogger<MarketplaceCategory> logger, IConfiguration configuration, IElasticClient client) : base(logger, configuration, client)
        {
        }

        public async Task<ServiceMessage<List<MarketplaceCategory>>> GetRootCategories(MarketplaceServiceMessage serviceMessage)
        {
            var response = new ServiceMessage<List<MarketplaceCategory>>(serviceMessage.Identity);

            try
            {
                var result = this.Client.Search<MarketplaceCategory>(s => s
                .Index(GetIndex(serviceMessage))
                .Query(x => !x.Exists(z => z.Field(a => a.ParentId))).Size(1000));

                if (result.ApiCall.HttpStatusCode == 200 || result.ApiCall.HttpStatusCode == 201 || result.ApiCall.HttpStatusCode == 404)
                    response.WithData(result.Documents.ToList());
                else
                    response.WithError(new Error(result.OriginalException));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while get root categories");
                Logger.LogCustomCritical(error, serviceMessage.Identity, null);
                response.WithError(error);
            }

            return response;
        }

        public override string FormatKey(MarketplaceServiceMessage message)
        {
            return $"category-{message.Marketplace.ToString().ToLower()}";
        }

        public override string FormatId(Identity identity, string id)
        {
            return $"{id}";
        }
    }
}

