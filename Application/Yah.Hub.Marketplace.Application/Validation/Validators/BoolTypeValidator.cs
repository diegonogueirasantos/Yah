using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IBoolTypeValidator : IFieldValidator { }

    public class BoolTypeValidator : FieldValidator, IBoolTypeValidator
    {
        public override string ErrorMessage { get; set; } = "O campo {field_displayname} deve conter um valor booleano com valores True ou False.";

        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            return (data is bool) || (data is bool?);
        }
    }
}
