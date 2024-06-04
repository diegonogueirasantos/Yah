using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IStringTypeValidator : IFieldValidator { }

    public class StringTypeValidator : FieldValidator, IStringTypeValidator
    {
        public override string ErrorMessage { get; set; } = "O campo {field_displayname} deve conter um valor do tipo String.";
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            return data is string;
        }
    }
}
