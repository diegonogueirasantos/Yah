using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IDecimalTypeValidator : IFieldValidator { }
    public class DecimalTypeValidator : FieldValidator, IDecimalTypeValidator
    {
        public override string ErrorMessage { get; set; } = "O campo {field_name} deve ser do tipo decimal.";

        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            bool isValid =  (data is decimal) || (data is double) || (data is float) || (data is Int32) || (data is Int64)
                            || (data is decimal?) || (data is double?) || (data is float?) || (data is Int32?) || (data is Int64?);

            if (data is string)
            {
                string dataAsString = (data as string);

                isValid = isValid
                    || (string.IsNullOrWhiteSpace(dataAsString))
                    || (!string.IsNullOrWhiteSpace(dataAsString) && (
                    decimal.TryParse(dataAsString, out decimal decimalValue)
                    || double.TryParse(dataAsString, out double doubleValue)
                    || float.TryParse(dataAsString, out float floatValue)));
            }

            return isValid;
        }
    }
}
