using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation
{
    public abstract class FieldValidator : IFieldValidator
    {

        public virtual string Name { get; set; }
        public virtual string[] Parameters { get; set; }
        public virtual string ErrorMessage { get; set; }
        public abstract bool Validate(dynamic data, ExpandoObject parameters = null);
    }
}
