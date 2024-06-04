using Yah.Hub.Common.Extensions;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IImageListLengthValidator : IFieldValidator { }
    public class ImageListLengthValidator : FieldValidator, IImageListLengthValidator
    {
        public override string ErrorMessage { get ; set ; }
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            var minlength = parameters.GetPropertyValue<int>("MinLength");

            this.ErrorMessage = $"A quantidade de imagens deve ser maior que {minlength}.";

            bool isValid = false;

            if(data is List<Image> images)
            {
                var totalImage = images.Count();
                isValid = totalImage >= minlength;
            }

            return isValid;
        }
    }
}
