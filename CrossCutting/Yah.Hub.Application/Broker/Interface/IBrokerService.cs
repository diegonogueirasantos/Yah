using Yah.Hub.Application.Broker.Messages;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Interface
{
    public interface IBrokerService
    {
        /// <summary>
        /// Publica mensagem que representa um comando a ser executado em um Marketplace.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ServiceMessage> EnqueueCommandAsync<T>(CommandMessage<T> message);


        public IEnumerable<CommandResult<T>[]> PeekCommand<T>(T message);

        ///// <summary>
        ///// Remove mensagens da fila de comandos, sinalizando que já foram processadas.
        ///// </summary>
        ///// <param name="message">Lista de dados de identificação das mensagens que devem ser removidas.</param>
        ///// <returns></returns>
        public Task<ServiceMessage<List<DequeueCommandResult>>> DequeueCommandBatchAsync<T>(ServiceMessage<DequeueCommandBatchMessage<T>> message);
    }
}
