using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliAddress
    {
        [JsonProperty("address_id")]
        public long? AddressId { get; set; }

        [JsonProperty("address_line")]
        public string AddressLine { get; set; }

        [JsonProperty("street_name")]
        public string StreetName { get; set; }

        [JsonProperty("street_number")]
        public string StreetNumber { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty("city")]
        public City City { get; set; }

        [JsonProperty("state")]
        public State State { get; set; }

        [JsonProperty("country")]
        public Country Country { get; set; }

        [JsonProperty("neighborhood")]
        public Place Neighborhood { get; set; }

        [JsonProperty("municipality")]
        public Place Municipality { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("latitude")]
        public decimal? Latitude { get; set; }

        [JsonProperty("longitude")]
        public decimal? Longitude { get; set; }

        [JsonProperty("geolocation_type")]
        public string GeolocationType { get; set; }
    }

    public class City
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class State
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class Country
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class Place
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
