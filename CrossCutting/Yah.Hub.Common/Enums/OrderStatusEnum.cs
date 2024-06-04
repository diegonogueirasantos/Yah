using Nest;
using System.ComponentModel.DataAnnotations;

namespace Yah.Hub.Common.Enums
{
    public enum OrderStatusEnum
    {
        [Display(Name = "Aguardando Pagamento")]
        WaitingPayment = 'W',

        [Display(Name = "Pago")]
        Paid = 'P',

        [Display(Name = "Faturado")]
        Invoiced = 'I',
        
        [Display(Name = "Enviado")]
        Shipped = 'S',
        
        [Display(Name = "Entregue")]
        Delivered = 'D',

        [Display(Name = "Cancelado")]
        Canceled = 'C',

        [Display(Name = "Reembolsado")]
        Refunded = 'R',

        [Display(Name = "Exceção de entrega")]
        ShipmentException,

        [Display(Name = "Aguardando Troca")]
        WaitingExchange,

        [Display(Name = "Aguardando Revisão")]
        WaitingReview
    }
}
