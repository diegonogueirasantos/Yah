using System;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Api.Application.MarketplaceControllerBase
{
	public abstract class MarketplaceControllerBase : ControllerBase
    {
        protected abstract MarketplaceAlias GetMarketplace();

        public virtual async Task<IServiceMessage> HandleAction(Func<Task<ServiceMessage>> method)
        {
            var result = await method();

            HandleAction(result.IsValid, result.Errors);

            return result;
        }

        public virtual async Task<IServiceMessage<T>> HandleAction<T>(Func<Task<ServiceMessage<T>>> method)
        {
            var result = await method();

            HandleAction(result.IsValid, result.Errors);

            return result;
        }

        public virtual async Task<IMarketplaceServiceMessage<T>> HandleAction<T>(Func<Task<MarketplaceServiceMessage<T>>> method)
        {
            var result = await method();

            HandleAction(result.IsValid, result.Errors);

            return result;
        }

        public virtual async Task<IMarketplaceServiceMessage> HandleAction(Func<Task<MarketplaceServiceMessage>> method)
        {
            var result = await method();

            HandleAction(result.IsValid, result.Errors);

            return result;
        }

        private void HandleAction(bool isValid, List<Error> errors)
        {
            if (isValid)
                return;
            else
                Response.StatusCode = 500;

            if (errors != null && errors.Any())
            {
                // handle errors here
                if (errors.Any(x => x.Type == ErrorType.Authentication))
                    Response.StatusCode = 401;
            }
        }
    }
}

