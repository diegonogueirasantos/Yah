namespace Yah.Hub.Domain.Catalog
{
    public class Price
    {
        /// <summary>
        /// Preço base do produto na lista de preços (Preço "DE").
        /// </summary>
        public decimal List { get; set; }

        /// <summary>
        /// Preço final que será cobrado do cliente no marketplace + taxas
        /// </summary>
        public decimal Retail { get; set; }

        /// <summary>
        /// Preço promocional que será cobrado do cliente no marketplace + taxas
        /// </summary>
        public decimal? SalePrice { get; set; }

        /// <summary>
        /// Data de inicio da vigência do preço promocional
        /// </summary>
        public DateTime? SalePriceFrom { get; set; }

        /// <summary>
        /// Data de fim da vigência do preço promocional
        /// </summary>
        public DateTime? SalePriceTo { get; set; }
    }
}
