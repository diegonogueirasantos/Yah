using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Manifest;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace Yah.Hub.Marketplace.Application.Validation
{
    public class ValidationService : AbstractService, IValidationService
    {
        public IValidatorFactory ValidatorFactory { get; }
        public ValidationService(
            IConfiguration configuration, 
            ILogger<ValidationService> logger,
            IValidatorFactory validatorFactory) 
            : base(configuration, logger)
        {
            ValidatorFactory = validatorFactory;
        }

        public async Task<MarketplaceServiceMessage<ProductIntegrationInfo>> Validate(MarketplaceServiceMessage<(Product Product, MarketplaceManifest Manifest)> message)
        {
            #region [Code]
            var product = message.Data.Product;
            var manifest = message.Data.Manifest;

            var productStatus = new ProductIntegrationInfo(product.Id, product.IntegrationId, EntityStatus.Unknown, DateTimeOffset.UtcNow);

            var result = new MarketplaceServiceMessage<ProductIntegrationInfo>(message.Identity, message.AccountConfiguration);

            foreach(var validationField in manifest.Fields)
            {
                if (validationField.Validations.Any())
                {
                    if (validationField.FieldLocation == FieldLocation.Sku)
                    {
                        foreach(var skuValidate in product.Skus)
                        {
                            productStatus.Errors.AddRange(await this.Validate(new ValidationContext()
                            {
                                Marketaplace = message.Marketplace.ToString(),
                                Product = skuValidate,
                                Field = validationField,
                                Sku = skuValidate.Id
                            }));
                        }
                    }
                    else
                    {
                        productStatus.Errors.AddRange(await this.Validate(new ValidationContext()
                        {
                            Marketaplace = message.Marketplace.ToString(),
                            Product = product,
                            Field = validationField,
                            Sku = product.Id
                        }));
                    }
                }
            }

            if (productStatus.Errors.Any())
            {
                productStatus.Status = EntityStatus.Declined;
            }

            result.WithData(productStatus);

            return result;
            #endregion
        }


        #region [Private Methods]

        private async Task<IEnumerable<IntegrationError>> Validate(ValidationContext context)
        {
            #region [Code]
            List<IntegrationError> errors = new List<IntegrationError>();

            var marketplaceField = context.Field;

            object objVal = this.TryGetPropertyValue(context.Product, marketplaceField.Name);

            if(objVal != null)
            {
                errors.AddRange(this.ExecuteValidation(objVal, context));

                foreach (var validation in context.Field.Validations)
                {
                    errors.AddRange(this.ExecuteValidation(objVal, context, validation));
                }
            }
                
            if (objVal == null && context.Field.IsRequired)
            {
                errors.Add(new IntegrationError() { ErrorMessage = $"A propriedade {marketplaceField.Name} está ausente no produto/sku {context.Sku}, ela é necessária para integração no marketplace {context.Marketaplace}" });
            }

            return errors;

            #endregion
        }

        private IEnumerable<IntegrationError> ExecuteValidation(dynamic propertyToValidate, ValidationContext context, Validations fieldValidation = null)
        {
            #region [Code]
            var errors = new List<IntegrationError>();

            string typeOfValidation = default;

            if(fieldValidation != null)
            {
                typeOfValidation = fieldValidation.ValidationType;
            }
            else
            {
                typeOfValidation = context.Field.FieldType;
            }

            IFieldValidator validator = this.ValidatorFactory.GetFieldValidator(typeOfValidation);

            bool isValid = validator.Validate(propertyToValidate, fieldValidation?.Params);

            if (!isValid)
            {
                yield return new IntegrationError
                {
                    ErrorMessage = this.BuildErrorMessage(validator.ErrorMessage, context),
                };
            }
            #endregion
        }

        private string BuildErrorMessage(string integrationInfo, ValidationContext context)
        {
            #region [Code]
            StringBuilder errorMessage = new StringBuilder(integrationInfo);

            errorMessage.Replace("{field_name}", $"{context.Field.Name}");
            errorMessage.Replace("{marketplace}", $"{context.Marketaplace}");
            errorMessage.Replace("{sku}", $"{context.Sku}");

            return errorMessage.ToString();
            #endregion
        }

        private object TryGetPropertyValue(object data, string path)
        {
            object objVal = null;

            if (path.Contains("."))
            {
                var propertiesArr = path.Split(".");

                foreach (var patchValue in propertiesArr)
                {
                    var prop = data.GetType();

                    PropertyInfo propInfo = prop.GetProperty(patchValue);

                    if (propInfo != null)
                    {
                        objVal = propInfo.GetValue(data, null);
                        data = objVal as object;
                    }
                }
            }
            else
            {
                var prop = data.GetType();

                PropertyInfo propInfo = prop.GetProperty(path);

                if (propInfo != null)
                {
                    objVal = propInfo.GetValue(data, null);
                }
            }

            return objVal;
        }

        #endregion
    }
}
