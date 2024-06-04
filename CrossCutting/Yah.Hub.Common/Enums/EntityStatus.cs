namespace Yah.Hub.Common.Enums
{
    public enum EntityStatus
    {
        /// <summary>
        /// Status desconhecido
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Aguardando envio para o marketplace
        /// </summary>
        Waiting = 1,
        /// <summary>
        /// Enviado porém pendente de verificação de status
        /// </summary>
        Sent = 2,
        /// <summary>
        /// Pendente de validação interna do marketplace.
        /// </summary>
        PendingValidation = 3,
        /// <summary>
        /// Rejeitado e não está integrado no marketplace.
        /// Existem erros associados que precisam ser corrigidos.
        /// </summary>
        Declined = 4,
        /// <summary>
        /// Aceito e está publicado/listado no marketplace.
        /// </summary>
        Accepted = 5,
        /// <summary>
        /// Venda do item foi parada no marketplace.
        /// </summary>
        Stopped = 6,
        /// <summary>
        /// Venda foi fechado no marketplace
        /// </summary>
        Closed = 7,
        /// <summary>
        /// Venda do item foi pausada no marketplace.
        /// </summary>
        Paused = 8,
    }
}
