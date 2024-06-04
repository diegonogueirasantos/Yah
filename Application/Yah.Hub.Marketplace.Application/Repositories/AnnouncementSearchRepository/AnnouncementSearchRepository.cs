using Nest;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;

namespace Yah.Hub.Data.Repositories.AnnouncementRepository
{
    public class AnnouncementSearchRepository : AbstractElasticSearchRepository<Domain.Announcement.Announcement>, IAnnouncementSearchRepository
    {
        public AnnouncementSearchRepository(ILogger<Announcement> logger, IConfiguration configuration, IElasticClient client) : base(logger, configuration, client)
        {
         
        }

        public override string FormatKey(MarketplaceServiceMessage message)
        {
            return $"announcement-{message.Identity.GetVendorId()}-{message.Identity.GetTenantId()}";
        }

        public async Task<ServiceMessage<AnnouncementSearchResult>> QueryAsync(MarketplaceServiceMessage<AnnouncementQuery> message)
        {
            var queryResult = await this.Client.SearchAsync<Announcement>(s => SearchByDescriptor(s, message)
                    .From(message.Data.Paging.Offset).Take(message.Data.Paging.Limit));

            if (!queryResult.IsValid && queryResult.ApiCall.HttpStatusCode != 404)
                return ServiceMessage<AnnouncementSearchResult>.CreateInvalidResult(message.Identity,new Error("Erro ao recuperar dados de busca.", "", ErrorType.Technical), new AnnouncementSearchResult());

            return ServiceMessage<AnnouncementSearchResult>.CreateValidResult(message.Identity,
                    new AnnouncementSearchResult()
                    {
                        Count = (int?)queryResult.Total ?? 0,
                        Offset = message.Data.Paging.Offset,
                        Limit = message.Data.Paging.Limit,
                        Docs = queryResult?.Documents?.ToArray(),
                    });
        }

        private SearchDescriptor<Announcement> SearchByDescriptor(SearchDescriptor<Announcement> searchDescriptor, MarketplaceServiceMessage<AnnouncementQuery> message)
        {
            #region [Code]

            var query = new QueryContainer();


            if (!String.IsNullOrEmpty(message.Data.TenantId))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.TenantId).Query($"{message.Data.TenantId}"));
            }
            else
            {
                throw new ArgumentNullException("TenantId", "Necessário informar um TenantId.");
            }

            if (!String.IsNullOrEmpty(message.Data.VendorId))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.VendorId).Query($"{message.Data.VendorId}"));
            }
            else
            {
                throw new ArgumentNullException("VendorId", "Necessário informar um VendorId.");
            }

            if (!String.IsNullOrEmpty(message.Data.Title))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.Item.Title).Query($"*{message.Data.Title}*"));
            }

            if (!String.IsNullOrEmpty(message.Data.AnnouncementId))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.Id).Query(message.Data.AnnouncementId));
            }

            if (!String.IsNullOrEmpty(message.Data.MarketplaceId))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.MarketplaceId).Query($"{message.Data.MarketplaceId}"));
            }

            if (!String.IsNullOrEmpty(message.Data.AccountId))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.AccountId).Query($"{message.Data.AccountId}"));
            }

            if (!String.IsNullOrEmpty(message.Data.ProductId))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.ProductId).Query($"{message.Data.ProductId}"));
            }

            if (!String.IsNullOrEmpty(message.Data.Category))
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.Item.CategoryId).Query($"{message.Data.Category}"));
            }

            if (message.Data.Status != null)
            {
                query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.Item.Status).Query(((int)message.Data.Status).ToString()));
            }

            query &= Query<Announcement>.QueryString(m => m.DefaultField(f => f.IsDeleted).Query("false"));

            var search = searchDescriptor
                            .Index(GetIndex(message))
                            .Query(q => query);

            return search
                    .Sort(sort => sort
                    .Field(f => f
                            .Field(x => x.Timestamp)
                            .Order(SortOrder.Descending))
                    .Field(f => f
                            .Field(x => x.Item.Title.Suffix("keyword"))
                            .Order(SortOrder.Ascending))
                    .Field(f => f
                            .Field(x => x.Id.Suffix("keyword"))
                            .Order(SortOrder.Descending)))
                    .TrackTotalHits(true);

            #endregion
        }
    }
}
