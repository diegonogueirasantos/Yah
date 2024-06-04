namespace Yah.Hub.Marketplace.Application.Broker.Messages.Interface
{
    public interface ICommandMessageHandler<T> where T : ICommandMessage
    {
        Task<bool> HandleCommand(T command);
    }
}
