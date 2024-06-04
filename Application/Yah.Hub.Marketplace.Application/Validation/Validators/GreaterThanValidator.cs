using Newtonsoft.Json.Linq;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IGreaterThanValidator : IFieldValidator { }
    public class GreaterThanValidator : FieldValidator, IGreaterThanValidator
    {
        public override string ErrorMessage { get; set; }

        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            var greaterThan = parameters.GetPropertyValue<int>("GreaterThan");

            this.ErrorMessage = $"O valor do campo {{field_displayname}} deve ser maior que {greaterThan}.";

            return (Convert.ChangeType(data, typeof(decimal)) > greaterThan);
        }
    }
}
