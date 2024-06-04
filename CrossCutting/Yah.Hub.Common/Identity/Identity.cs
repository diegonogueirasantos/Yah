using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Yah.Hub.Common.Exceptions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;

namespace Yah.Hub.Common.Identity
{
    public class Identity 
    {
        #region [Constraint]
        public const string Actor = "ClaimType";
        #endregion

        #region [Constructor]
        public Identity(Dictionary<string, string> claims)
        {
            Claims = claims;
        }
        #endregion

        #region [Properties]
        public Dictionary<string,string> Claims { get; set; }

        #endregion

        #region [Methods]
        public virtual bool HasClaim(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(key));
            }



            return Claims.GetValueOrDefault(key).Equals(value);

        }

        #endregion
    }

    public class MarketplaceIdentity
    {
        public MarketplaceAlias Marketplace { get; set; } 

        public MarketplaceIdentity(MarketplaceAlias marketplace)
        {
            this.Marketplace = marketplace;
        }
    }
}


