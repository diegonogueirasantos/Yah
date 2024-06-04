using Yah.Hub.Common.Extensions;

namespace Yah.Hub.Domain.Order
{
    public class Address
    {
        public string AddressLine { get; set; }
        public string Number { get; set; }
        public string PostalCode { get; set; }
        public string Neighbourhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string AddressNotes { get; set; }
        public string Landmark { get; set; }
        public string FullName { get; set; }


        public static readonly IEnumerable<KeyValuePair<string, string[]>> StateMapper = new KeyValuePair<string, string[]>[27] {
            new KeyValuePair<string, string[]>("AC", new string[2] { "Acre", "AC" }),
            new KeyValuePair<string, string[]>("AL", new string[2] { "Alagoas", "AL" }),
            new KeyValuePair<string, string[]>("AP", new string[2] { "Amapá", "AP" }),
            new KeyValuePair<string, string[]>("AM", new string[2] { "Amazonas", "AM" }),
            new KeyValuePair<string, string[]>("BA", new string[2] { "Bahia", "BA" }),
            new KeyValuePair<string, string[]>("CE", new string[2] { "Ceará", "CE" }),
            new KeyValuePair<string, string[]>("DF", new string[2] { "Distrito Federal", "DF" }),
            new KeyValuePair<string, string[]>("ES", new string[2] { "Espírito Santo", "ES" }),
            new KeyValuePair<string, string[]>("GO", new string[2] { "Goiás", "GO" }),
            new KeyValuePair<string, string[]>("MA", new string[2] { "Maranhão", "MA" }),
            new KeyValuePair<string, string[]>("MT", new string[2] { "Mato Grosso", "MT" }),
            new KeyValuePair<string, string[]>("MS", new string[2] { "Mato Grosso do Sul", "MS" }),
            new KeyValuePair<string, string[]>("MG", new string[2] { "Minas Gerais", "MG" }),
            new KeyValuePair<string, string[]>("PA", new string[2] { "Pará", "PA" }),
            new KeyValuePair<string, string[]>("PB", new string[2] { "Paraíba", "PB" }),
            new KeyValuePair<string, string[]>("PR", new string[2] { "Paraná", "PR" }),
            new KeyValuePair<string, string[]>("PE", new string[2] { "Pernambuco", "PE" }),
            new KeyValuePair<string, string[]>("PI", new string[2] { "Piauí", "PI" }),
            new KeyValuePair<string, string[]>("RJ", new string[2] { "Rio de Janeiro", "RJ" }),
            new KeyValuePair<string, string[]>("RN", new string[2] { "Rio Grande do Norte", "RN" }),
            new KeyValuePair<string, string[]>("RS", new string[2] { "Rio Grande do Sul", "RS" }),
            new KeyValuePair<string, string[]>("RO", new string[2] { "Rondônia", "RO" }),
            new KeyValuePair<string, string[]>("RR", new string[2] { "Roraima", "RR" }),
            new KeyValuePair<string, string[]>("SC", new string[2] { "Santa Catarina", "SC" }),
            new KeyValuePair<string, string[]>("SP", new string[2] { "São Paulo", "SP" }),
            new KeyValuePair<string, string[]>("SE", new string[2] { "Sergipe", "SE" }),
            new KeyValuePair<string, string[]>("TO", new string[2] { "Tocantins", "TO" }),
        };

        public Address StateNameToUF(string stateName = null)
        {
            if (string.IsNullOrWhiteSpace(stateName))
                stateName = State ?? string.Empty;

            State = StateMapper
                .FirstOrDefault(x => x.Value.Select(n => n.ToLowerInvariant().RemoveAccents())
                    .Contains(stateName.ToLowerInvariant().RemoveAccents()))
                .Key ?? stateName.Truncate(2);

            return this;
        }
    }


}
