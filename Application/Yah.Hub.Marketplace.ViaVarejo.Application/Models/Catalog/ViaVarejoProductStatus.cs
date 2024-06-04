namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public enum ViaVarejoProductStatus
    {
        /// <summary>
        /// Produto aguarda validação da Via Varejo, pode ser atualizado
        /// </summary>
        AGUARDANDO_PROCESSAMENTO,
        /// <summary>
        /// Produto possui erros a serem corrigidos
        /// </summary>
        INVALIDO,
        /// <summary>
        /// Produto já não possui erros mas ainda não está com a integração completa
        /// </summary>
        VALIDO,
        /// <summary>
        /// Produto já está integrado com a Via Varejo, produto não pode mais ser alterado, apenas preço, estoque e tempo de manuseio
        /// </summary>
        FICHA_INTEGRADA,
    }
}
