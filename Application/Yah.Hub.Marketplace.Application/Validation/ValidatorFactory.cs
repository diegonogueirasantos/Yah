using Yah.Hub.Marketplace.Application.Validation.Interface;

namespace Yah.Hub.Marketplace.Application.Validation
{
    public class ValidatorFactory : IValidatorFactory
    {
        #region Properties
        private IServiceProvider ServiceProvider { get; }

        private Dictionary<string, Type> ValidatorTypes { get; }
        #endregion

        #region Constructor
        public ValidatorFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;

            this.ValidatorTypes = AppDomain.CurrentDomain.GetAssemblies()
                   .Where(x => x.FullName.Contains("Yah.Hub.Marketplace.Application"))
                   .SelectMany(x => x.GetExportedTypes().Where(t => typeof(IFieldValidator).IsAssignableFrom(t) && t.IsInterface))
                   .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
        }
        #endregion

        public IFieldValidator GetFieldValidator(string interfaceType)
        {
            #region [Code]
            this.ValidatorTypes.TryGetValue(interfaceType, out Type validatorType);

            return (IFieldValidator)this.ServiceProvider.GetService(validatorType);
            #endregion
        }
    }
}
