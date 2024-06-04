using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IImageListTypeValidator : IFieldValidator { }
    public class ImageListTypeValidator : FieldValidator, IImageListTypeValidator
    {
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            bool isValid = (data == null) || (data is List<Domain.Catalog.Image>);

            return isValid;
        }
    }
}
