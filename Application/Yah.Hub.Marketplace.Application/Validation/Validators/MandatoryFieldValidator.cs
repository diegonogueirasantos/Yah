using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IMandatoryFieldValidator : IFieldValidator { }

    public class MandatoryFieldValidator : FieldValidator, IMandatoryFieldValidator
    {
        public override string ErrorMessage { get; set; } = "O campo {field_displayname} é necessário para o marketplace {marketplace}.";

        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            return !(data == null || (data is string && String.IsNullOrWhiteSpace(data as string)));
        }
    }
}
