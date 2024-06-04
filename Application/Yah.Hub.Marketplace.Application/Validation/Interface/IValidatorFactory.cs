namespace Yah.Hub.Marketplace.Application.Validation.Interface
{
    public interface IValidatorFactory
    {
        IFieldValidator GetFieldValidator(string interfaceType);
    }
}
