using Yah.Hub.Common.Enums;

namespace Yah.Hub.Domain.Order
{
    public class Customer
    {
        public CustomerType Type { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public Gender? Gender { get; set; }
        public string DocumentNumber { get; set; }
        public string Cellphone { get; set; }
        public string Phone { get; set; }
        public string TradingName { get; set; }
        public string StateInscription { get; set; }
    }
}
