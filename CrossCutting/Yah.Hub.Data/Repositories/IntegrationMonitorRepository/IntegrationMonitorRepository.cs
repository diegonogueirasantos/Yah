using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nest;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.Query;

namespace Yah.Hub.Data.Repositories.IntegrationMonitorRepository
{
    public class IntegrationMonitorRepository : AbstractElasticSearchRepository<MarketplaceEntityState>, IIntegrationMonitorRepository
    {
        private readonly IDictionary<BatchType, Expression<Func<ProductIntegrationInfo, object>>> BatchIdExpression;
        public IntegrationMonitorRepository(ILogger<MarketplaceEntityState> logger, IConfiguration configuration, IElasticClient client) : base(logger, configuration, client)
        {
            BatchIdExpression = new Dictionary<BatchType, Expression<Func<ProductIntegrationInfo, object>>>()
            {
                { BatchType.PRODUCT, f => f.BatchId.Suffix("keyword") },
                { BatchType.PRICE, f => f.Skus.Select(x => x.PriceIntegrationInfo.BatchId.Suffix("keyword")) },
                { BatchType.INVENTORY, f => f.Skus.Select(x => x.InventoryIntegrationInfo.BatchId.Suffix("keyword")) },
            };
        }

        public override string FormatKey(MarketplaceServiceMessage message)
        {
            return $"{message.Identity.GetVendorId()}-{message.Identity.GetTenantId()}-entity-state";
        }

        public async Task<ServiceMessage<List<MarketplaceEntityState>>> GetByReferenceId(MarketplaceServiceMessage<string> message)
        {
            #region [Code]
            var queryResult = await this.Client.SearchAsync<MarketplaceEntityState>(s => SearchByDescriptor(s, message));

            if (!queryResult.IsValid)
                return ServiceMessage<List<MarketplaceEntityState>>.CreateInvalidResult(message.Identity, new Error("Erro ao recuperar dados de busca.", "", ErrorType.Technical), new List<MarketplaceEntityState>());

            return ServiceMessage<List<MarketplaceEntityState>>.CreateValidResult(message.Identity,
                    queryResult.Documents.ToList());
            #endregion
        }

        public SearchDescriptor<MarketplaceEntityState> SearchByDescriptor(SearchDescriptor<MarketplaceEntityState> searchDescriptor, MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]

            var query = new QueryContainer();


            if (!String.IsNullOrEmpty(marketplaceServiceMessage.Identity.GetVendorId()))
            {
                query &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.VendorId).Query($"{marketplaceServiceMessage.Identity.GetVendorId()}"));
            }
            else
            {
                throw new ArgumentNullException("TenantId", "Necessário informar um TenantId.");
            }

            if (!String.IsNullOrEmpty(marketplaceServiceMessage.Identity.GetTenantId()))
            {
                query &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.TenantId).Query($"{marketplaceServiceMessage.Identity.GetTenantId()}"));
            }
            else
            {
                throw new ArgumentNullException("VendorId", "Necessário informar um VendorId.");
            }

            if (!String.IsNullOrEmpty(marketplaceServiceMessage.Identity.GetAccountId()))
            {
                query &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.AccountId).Query($"{marketplaceServiceMessage.Identity.GetAccountId()}"));
            }
            else
            {
                throw new ArgumentNullException("VendorId", "Necessário informar um VendorId.");
            }

            if (!String.IsNullOrEmpty(marketplaceServiceMessage.Data))
            {
                query &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.ReferenceId).Query($"{marketplaceServiceMessage.Data}"));
            }

            var search = searchDescriptor
                            .Index(GetIndex(marketplaceServiceMessage))
                            .Query(q => query);

            return search
                    .Sort(sort => sort
                    .Field(f => f.Field("_score"))
                    .Field(f => f
                            .Field(x => x.DateTime)
                            .Order(SortOrder.Descending))
                    .Field(f => f
                            .Field(x => x.Id.Suffix("keyword"))
                            .Order(SortOrder.Descending)))
                    .TrackTotalHits(true);

            #endregion
        }

        public async Task<ServiceMessage<List<IntegrationSummary>>> GetIntegrationSummary(MarketplaceServiceMessage serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<List<IntegrationSummary>>(serviceMessage.Identity);
            result.Data = new List<IntegrationSummary>();

            try
            {
                var searchResult = this.Client.Search<MarketplaceEntityState>(s => s
                .Index(GetIndex(serviceMessage))
                .Query(x => x.Match(z => z.Field(a => a.VendorId == serviceMessage.Identity.GetVendorId() && a.TenantId == serviceMessage.Identity.GetTenantId())))
                    .Aggregations(a => a
                        .Terms("Accounts", ts => ts
                            .Field(o => o.AccountId.Suffix("keyword")) // use the keyword sub-field for terms aggregation
                            .Size(int.MaxValue)
                            .Aggregations(aa => aa
                                .Terms("ProductStatus", sa => sa
                                    .Field(o => o.ProductInfo.Status)
                                ).Terms("VendorId", sb => sb.Field(p => p.VendorId.Suffix("keyword"))
                                ).Terms("MarketplaceAlias", sb => sb.Field(p => p.MarketplaceAlias)
                                ).Terms("TenantId", sb => sb.Field(p => p.TenantId.Suffix("keyword")))
                            )
                        )
                    )
                );

                if (searchResult.IsValid)
                {
                    foreach (BucketAggregate bucket in searchResult.Aggregations.Values)
                    {
                        foreach (KeyedBucket<object> item in bucket.Items)
                        {
                            //keys
                            var singleAgg = new IntegrationSummary(
                                ((KeyedBucket<object>)((BucketAggregate)item.GetValueOrDefault("VendorId")).Items.First()).Key.ToString(),
                                ((KeyedBucket<object>)((BucketAggregate)item.GetValueOrDefault("TenantId")).Items.First()).Key.ToString(),
                                (string)item.Key,
                                (MarketplaceAlias)Convert.ToInt32(((KeyedBucket<object>)((BucketAggregate)item.GetValueOrDefault("MarketplaceAlias")).Items.First()).Key));

                            List<IBucket> ibucketState = ((BucketAggregate)item.GetValueOrDefault("ProductStatus")).Items.ToList();
                            List<KeyedBucket<object>> statuses = ibucketState.Select(x => ((KeyedBucket<object>)x)).ToList();

                            singleAgg.Waiting = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Waiting)?.DocCount ?? default(Int64);
                            singleAgg.Unknown = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Unknown)?.DocCount ?? default(Int64);
                            singleAgg.Accepted = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Accepted)?.DocCount ?? default(Int64);
                            singleAgg.Closed = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Closed)?.DocCount ?? default(Int64);
                            singleAgg.Declined = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Declined)?.DocCount ?? default(Int64);
                            singleAgg.PendingValidation = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.PendingValidation)?.DocCount ?? default(Int64);
                            singleAgg.Stopped = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Stopped)?.DocCount  ?? default(Int64);
                            singleAgg.Paused = (Int64?)statuses.FirstOrDefault(x => (Int64)x.Key == (Int64)EntityStatus.Paused)?.DocCount ?? default(Int64);
                            result.Data.Add(singleAgg);
                        };
                    }
                }
                else
                    result.WithError(new Error(searchResult.OriginalException ?? new Exception("ElasticSearchError")));
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
            #endregion
        }

        public async Task<ServiceMessage<EntityStateSearchResult>> QueryAsync(MarketplaceServiceMessage<EntityStateQuery> message)
        {
            #region [Code]
            var queryResult = await this.Client.SearchAsync<MarketplaceEntityState>(s => SearchByDescriptor(s, message)
                    .From(message.Data.Paging.Offset).Take(message.Data.Paging.Limit));

            if (!queryResult.IsValid)
                return ServiceMessage<EntityStateSearchResult>.CreateInvalidResult(message.Identity, new Error("Erro ao recuperar dados de busca.", "", ErrorType.Technical), new EntityStateSearchResult());

            return ServiceMessage<EntityStateSearchResult>.CreateValidResult(message.Identity,
                    new EntityStateSearchResult()
                    {
                        Count = (int)queryResult.Total,
                        Offset = message.Data.Paging.Offset,
                        Limit = message.Data.Paging.Limit,
                        Docs = queryResult.Documents.ToArray(),
                    });
            #endregion
        }

        private SearchDescriptor<MarketplaceEntityState> SearchByDescriptor(SearchDescriptor<MarketplaceEntityState> searchDescriptor, MarketplaceServiceMessage<EntityStateQuery> message)
        {
            #region [Code]

            var queryAnd = new QueryContainer();
            var queryOr = new QueryContainer();
            var queryMain = new QueryContainer();


            if (!String.IsNullOrEmpty(message.Data.TenantId))
                queryAnd &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.TenantId).Query(message.Data.TenantId));
            else
                throw new ArgumentNullException("TenantId", "Necessário informar um TenantId.");


            if (!String.IsNullOrEmpty(message.Data.VendorId))
                queryAnd &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.VendorId).Query(message.Data.VendorId));
            else
                throw new ArgumentNullException("VendorId", "Necessário informar um VendorId.");


            if (!String.IsNullOrEmpty(message.Data.AccountId))
                queryAnd &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.AccountId).Query(message.Data.AccountId));
            else
                throw new ArgumentNullException("AccountId", "Necessário informar um AccountId.");

            if (!String.IsNullOrEmpty(message.Data.Id))
                queryAnd &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.Id).Query(message.Data.Id));

            if (!String.IsNullOrEmpty(message.Data.ReferenceId))
                queryAnd &= Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => f.Id).Query(message.Data.ReferenceId));


            if (message.Data.HasErrors)
                queryAnd &= new ExistsQuery() { Field = "productInfo.errors" };//Query<MarketplaceEntityState>.(m => m.Field(x => x.ProductInfo.Errors));


            if (message.Data.Statuses != null && message.Data.Statuses.Any())
            {
                foreach (var status in message.Data.Statuses)
                {
                    if (message.Data.Statuses.IndexOf(status) == 0)
                        queryOr = new TermQuery() { Field = "productInfo.status", Value = (int)status };//  Query<MarketplaceEntityState>.QueryString(m => m.DefaultField(f => ((int)f.ProductInfo.Status).ToString()).Query(((int)status).ToString()));
                    else
                        queryOr |= new TermQuery() { Field = "productInfo.status", Value = (int)status };
                }
            }
            queryMain = queryAnd & queryOr;


            var search = searchDescriptor
                               .Index(GetIndex(message))
                               .Query(q => queryMain);

            return search
                    .Sort(sort => sort
                    .Field(f => f.Field("_score"))
                    .Field(f => f
                            .Field(x => x.DateTime)
                            .Order(SortOrder.Descending))
                    .Field(f => f
                            .Field(x => x.Id.Suffix("keyword"))
                            .Order(SortOrder.Descending)))
                    .TrackTotalHits(true);

            #endregion
        }

        public async Task<ISearchResponse<MarketplaceEntityState>> GetEntitiesByStatusRequest(MarketplaceServiceMessage<MonitorStatusRequest> message, EntityType type)
        {
            #region [Code]
            switch (type)
            {
                case EntityType.Product:
                    return await this.GetProductsByStatus(message);
                case EntityType.Price:
                    return await this.GetPricesByStatus(message);
                case EntityType.Inventory:
                    return await this.GetInventoriesByStatus(message);
                default:
                    throw new NotImplementedException();
            }
            #endregion
        }

        private async Task<ISearchResponse<MarketplaceEntityState>> GetProductsByStatus(MarketplaceServiceMessage<MonitorStatusRequest> message)
        {
            #region [Code]
            var query = new QueryContainer();

            query &= Query<MarketplaceEntityState>.Match(m => m.Field(f => f.ProductInfo.Status).Query("1"));

            query |= Query<MarketplaceEntityState>.Bool(
                b => b.Must(
                    m => m.Match(
                        f => f.Field(ff => ff.ProductInfo.Status)
                        .Query("0"))));

            query |= Query<MarketplaceEntityState>.Bool(
                b => b.Must(
                    m => m.Match(
                        f => f.Field(ff => ff.ProductInfo.Status)
                        .Query("3"))));

            return await this.Client.SearchAsync<MarketplaceEntityState>(
                s => s
                .Index(GetIndex(message))
                .Scroll(message.Data.ScrollTime)
                .Size(message.Data.MaxItemsPerExecution)
                .Query(q => query));
            #endregion
        }

        private async Task<ISearchResponse<MarketplaceEntityState>> GetPricesByStatus(MarketplaceServiceMessage<MonitorStatusRequest> message)
        {
            #region [Code]
            var query = new QueryContainer();

            query &= Query<MarketplaceEntityState>.Match(m => m.Field(f => f.ProductInfo.Skus.Select(x => x.PriceIntegrationInfo.Status)).Query("1"));

            query |= Query<MarketplaceEntityState>.Bool(
                b => b.Must(
                    m => m.Match(
                        f => f.Field(ff => ff.ProductInfo.Skus.Select(x => x.PriceIntegrationInfo.Status))
                        .Query("0"))));

            query |= Query<MarketplaceEntityState>.Bool(
                b => b.Must(
                    m => m.Match(
                        f => f.Field(ff => ff.ProductInfo.Skus.Select(x => x.PriceIntegrationInfo.Status))
                        .Query("3"))));

            return await this.Client.SearchAsync<MarketplaceEntityState>(
                s => s
                .Index(GetIndex(message))
                .Scroll(message.Data.ScrollTime)
                .Size(message.Data.MaxItemsPerExecution)
                .Query(q => query));
            #endregion
        }

        private async Task<ISearchResponse<MarketplaceEntityState>> GetInventoriesByStatus(MarketplaceServiceMessage<MonitorStatusRequest> message)
        {
            #region [Code]
            var query = new QueryContainer();

            query &= Query<MarketplaceEntityState>.Match(m => m.Field(f => f.ProductInfo.Skus.Select(x => x.InventoryIntegrationInfo.Status)).Query("1"));

            query |= Query<MarketplaceEntityState>.Bool(
                b => b.Must(
                    m => m.Match(
                        f => f.Field(ff => ff.ProductInfo.Skus.Select(x => x.InventoryIntegrationInfo.Status))
                        .Query("0"))));

            query |= Query<MarketplaceEntityState>.Bool(
                b => b.Must(
                    m => m.Match(
                        f => f.Field(ff => ff.ProductInfo.Skus.Select(x => x.InventoryIntegrationInfo.Status))
                        .Query("3"))));

            return await this.Client.SearchAsync<MarketplaceEntityState>(
                s => s
                .Index(GetIndex(message))
                .Scroll(message.Data.ScrollTime)
                .Size(message.Data.MaxItemsPerExecution)
                .Query(q => query));
            #endregion
        }

        public async Task<ISearchResponse<MarketplaceEntityState>> GetEntitiesByBatchId(MarketplaceServiceMessage<BatchQueryRequest> message)
        {
            #region [Code]
            var query = new QueryContainer();

            query &= Query<MarketplaceEntityState>.Match(m => m.Field(BatchIdExpression[message.Data.BatchType]).Query(message.Data.BatchId));


            return await this.Client.SearchAsync<MarketplaceEntityState>(
                s => s
                .Index(GetIndex(message))
                .Scroll(message.Data.ScrollTime)
                .Size(message.Data.MaxItemsPerExecution)
                .Query(q => query));
            #endregion
        }
    }
}