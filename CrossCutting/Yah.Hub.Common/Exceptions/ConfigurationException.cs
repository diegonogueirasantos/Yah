using System;
using Yah.Hub.Common.Identity;

namespace Yah.Hub.Common.Exceptions
{
    public class ConfigurationException : Exception
    {
        public IIdentity Identity { get; private set; }

        public ConfigurationException(IIdentity Identity)
        {
            this.Identity = Identity;
        }
    }
}

