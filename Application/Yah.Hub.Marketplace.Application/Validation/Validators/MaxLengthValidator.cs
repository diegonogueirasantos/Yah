using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IMaxLengthValidator : IFieldValidator { }
    public class MaxLengthValidator : FieldValidator, IMaxLengthValidator
    {
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            var maxLenght = parameters.GetPropertyValue<int>("MaxLength");

            var text = data.ToString();

            bool isValid = text.Length <= maxLenght;

            return isValid;
        }
    }
}
