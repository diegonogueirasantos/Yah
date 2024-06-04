using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Interface
{
    public interface IFieldValidator
    {
        string Name { get; }
        string ErrorMessage { get; }
        string[] Parameters { get; }

        bool Validate(dynamic data, ExpandoObject parameters = null);

    }
}
