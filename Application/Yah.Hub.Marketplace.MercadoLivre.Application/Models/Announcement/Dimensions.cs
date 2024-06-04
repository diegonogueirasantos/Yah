using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class Dimensions
    {
        [JsonIgnore]
        public double Height { get; set; }

        [JsonIgnore]
        public double Length { get; set; }

        [JsonIgnore]
        public double Width { get; set; }

        [JsonIgnore]
        public decimal Weight { get; set; }

        public override string ToString()
        {
            return $"{Height.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)}x" +
                   $"{Length.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)}x" +
                   $"{Width.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)}," +
                   $"{Weight.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)}";
        }
    }

    public class DimensionsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dimensions);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || string.IsNullOrWhiteSpace(reader.Value.ToString()))
            {
                return new Dimensions();
            }
            else
            {
                var dimensions = reader.Value.ToString().Split('x');
                if (dimensions.Length == 3 && dimensions[2].Split(',').Length == 2)
                {
                    string weight = dimensions[2].Split(',')[1];
                    dimensions[2] = dimensions[2].Split(',')[0];
                    return new Dimensions() { Height = double.Parse(dimensions[0]), Length = double.Parse(dimensions[1]), Width = double.Parse(dimensions[2]), Weight = decimal.Parse(weight) };
                }
                else
                    return new Dimensions();
            }
        }
    }
}
