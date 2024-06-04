using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Query;

namespace Yah.Hub.Domain.Monitor
{
    public class EntityStateQuery : BaseQuery
    {
        public EntityStateQuery() { }

        public List<EntityStatus> Statuses { get; set; }
        public bool HasErrors { get; set; }
        public string VendorId { get; set; }
        public string TenantId { get; set; }
        public string AccountId { get; set; }
        public string Id { get; set; }
        public string ReferenceId { get; set; }
    }
}
