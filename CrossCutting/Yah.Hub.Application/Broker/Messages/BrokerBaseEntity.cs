using System;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Broker.Messages
{
    public class BrokerBaseEntity<T> 
    {
        public BrokerBaseEntity()
        {
            this.CommandDataType = typeof(T).Name.ToLower();
            this.AssemblyQualifiedName = typeof(CommandMessage<T>).AssemblyQualifiedName;
        }

        public BrokerBaseEntity(T data) 
        {
            this.Data = data;
            this.CommandDataType = typeof(T).Name.ToLower();
            this.AssemblyQualifiedName = typeof(CommandMessage<T>).AssemblyQualifiedName;
        }

        public Identity Identity { get; set; }

        /// <summary>
        /// Objeto do tipo que foi definido na criação do comando
        /// </summary>        
        public T Data { get; set; }

        /// <summary>
        /// Tipo do objeto contido no comando.
        /// </summary>
        /// <example>
        /// "product", "inventory", "price", "product", "order"
        /// </example>
        public string CommandDataType { get; private set; }
        /// <summary>
        /// Tipo do objeto contido no comando.
        /// </summary>
        /// <example>
        /// "product", "inventory", "price", "product", "order"
        /// </example>
        public string AssemblyQualifiedName { get; private set; }

        public MarketplaceAlias Marketplace { get; set; }

    }
}

