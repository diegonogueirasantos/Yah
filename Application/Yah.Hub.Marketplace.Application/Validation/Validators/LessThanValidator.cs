using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface ILessThanValidator : IFieldValidator { }
    public class LessThanValidator : FieldValidator, ILessThanValidator
    {
        public override string ErrorMessage { get ; set ; }
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            var max = parameters.GetPropertyValue<int>("LessThan");

            this.ErrorMessage = $"O valor do campo {{field_displayname}} deve ser menor ou igual a {max}.";

            return data.Length <= max;
        }
    }
}
