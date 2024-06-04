using Yah.Hub.Common.Extensions;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using System.Dynamic;
using System.Text;

namespace Yah.Hub.Marketplace.Application.Validation.Validators
{
    public interface IImageSizeValidator : IFieldValidator { }
    public class ImageSizeValidator : FieldValidator, IImageSizeValidator
    {
        public override string ErrorMessage { get; set; }
        public override bool Validate(dynamic data, ExpandoObject parameters = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Existem imagens fora do padrão de medidas aceito pelo marketplace {marketplace}");
            var minWidth = parameters.GetPropertyValue<int>("MinWidth");
            var minHeight = parameters.GetPropertyValue<int>("MinHeight");
            var maxWidth = parameters.GetPropertyValue<int>("MaxWidth");
            var maxHeight = parameters.GetPropertyValue<int>("MaxHeight");

            bool isValid = true;

            if(data is List<Image> images)
            {
                bool hasMinValidationError = images.Any(image => (image.Height < minHeight || image.Width < minWidth));

                if (hasMinValidationError)
                {
                    stringBuilder.AppendLine($" Tamanho minímo {minWidth}px X {minHeight}px.");
                    isValid = false;
                }
                    

                if((maxHeight > default(int) && maxWidth > default(int)))
                {
                    bool hasMaxValidationError = images.Any(image => (image.Height > maxHeight || image.Width > maxWidth));

                    if (hasMaxValidationError)
                    {
                        stringBuilder.AppendLine($" Tamanho máximo {maxWidth}px X {maxHeight}px.");
                        isValid = false;
                    }
                }

            }

            this.ErrorMessage = stringBuilder.ToString();

            return isValid;
        }
    }
}
