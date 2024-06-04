using Newtonsoft.Json;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class Product
    {
        /// <summary>
        /// O código informado aqui, é obrigatório para 100% dos itens. 
        /// Produto simples: Passar um código diferente para cada SKU/Produtos 
        /// Variantes: Passar o mesmo código para todas as variantes do mesmo item.
        /// </summary>
        [JsonProperty("idItem")]
        public string ItemId { get; set; }

        /// <summary>
        /// O campo aceita um máximo de 150 caracteres. Algumas palavras podem ser rejeitadas no momento da validação por não ser permitida.
        /// </summary>
        [JsonProperty("titulo")]
        public string Title { get; set; }

        /// <summary>
        /// Formatação: HTML padrão UTF-8. Quebra de linha padrão: <br> e separador de parágrafo: <p>. 
        /// Não permitimos passar: imagens e links. Máximo de 18.000 caracteres. 
        /// Algumas palavras podem ser rejeitadas no momento da validação por não ser permitida.
        /// </summary>
        [JsonProperty("descricao")]
        public string Description { get; set; }

        /// <summary>
        /// O campo aceita um máximo de 50 caracteres. 
        /// Marcas proibidas: Algumas marcas podem ser rejeitadas no momento da validação por não haver permissão para o seller comercializá-la no marketplace.
        /// </summary>
        [JsonProperty("marca")]
        public string Brand { get; set; }

        /// <summary>
        /// Deve-se informar apenas o ID do último nível da categoria que deseja cadastrar o produto.
        /// Caso seja passado: um código inexistente, código que não seja do último nível da categoria, 
        /// sem valor (em branco) ou não seja enviado, cadastraremos em uma categoria genérica chamada Produtos Diversos (id: 3163).
        /// </summary>
        [JsonProperty("idCategoria")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Deve-se indicar nesse array, quais atributos para enriquecimento da ficha (caracteristicas, especificações técnicas e informações adicionais) 
        /// deseja cadastrar para o produto
        /// </summary>
        [JsonProperty("atributos")]
        public List<Attribute> Attributes { get; set; }

        [JsonProperty("skus")]
        public List<Sku> Skus { get; set; }

        /// <summary>
        /// Garantia do fabricante do produto. 
        /// O valor deve ser númerico de 1 à 60 considerando que a referência é de MESES. 
        /// Este campo somente deve ser preenchido quando para a categoria indicada no campo idCategoria, exigí-lo como obrigatório.
        /// </summary>
        [JsonProperty("garantia")]
        public int Warranty { get; set; }
    }
}
