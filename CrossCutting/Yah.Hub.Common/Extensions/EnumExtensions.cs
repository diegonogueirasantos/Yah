using Yah.Hub.Common.Enums;

namespace Yah.Hub.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string Translate(this WarrantyType warranty)
        {
            switch (warranty)
            {
                case WarrantyType.day:
                    return "dia";
                case WarrantyType.month:
                    return "mês";
                case WarrantyType.year:
                    return "ano";
                    default:
                    return "dia";
            }
        }
    }
}
