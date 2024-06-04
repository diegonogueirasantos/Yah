using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IIntTypeValidator : IFieldValidator { }
    public class IntTypeValidator : FieldValidator, IIntTypeValidator
    {
        public override string ErrorMessage { get; set; } = "O campo {field_displayname} deve conter um valor do tipo Inteiro.";
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            bool isValid = (data == null) || (data is int) || (data is int?);

            if (data is string)
            {
                string dataAsString = (data as string);

                isValid = isValid || (string.IsNullOrWhiteSpace(dataAsString))
                                  || (!string.IsNullOrWhiteSpace(dataAsString)
                                  && int.TryParse(dataAsString, out int intValue));
            }

            return isValid;
        }
    }
}
