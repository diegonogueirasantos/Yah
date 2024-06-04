using Nest;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Category
{
    public class SearchCategory
    {
        public SearchCategory(int offset, int limit, string id = null)
        {
            Offset = offset;
            Limit = limit;
            Id = id;
        }

        public string? Id { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }
    }
}
