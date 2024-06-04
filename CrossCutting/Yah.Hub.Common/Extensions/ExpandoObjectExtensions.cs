using System.Dynamic;
using System;

namespace Yah.Hub.Common.Extensions
{
    public static class ExpandoObjectExtensions
    {
        public static dynamic GetPropertyValue(this ExpandoObject obj, string propertyName)
        {
            return obj.GetPropertyValue<dynamic>(propertyName);
        }

        public static TResult GetPropertyValue<TResult>(this ExpandoObject obj, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentOutOfRangeException(nameof(propertyName));

            if (obj == null)
                return default;

            dynamic value = obj.GetPropertyByPath(propertyName, out bool propertyExists);
            if (value == null)
                return default;

            try
            {
                if (typeof(TResult) == typeof(object))
                    return value;

                return (TResult)Convert.ChangeType(value, typeof(TResult));
            }
            catch (Exception e)
            {
                return default;
            }
        }

        private static dynamic GetPropertyByPath(this ExpandoObject obj, string propertyPath, out bool propertyExists)
        {
            if (String.IsNullOrWhiteSpace(propertyPath))
                throw new ArgumentOutOfRangeException(nameof(propertyPath));

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            Newtonsoft.Json.Linq.JObject jsonObject = Newtonsoft.Json.Linq.JObject.Parse(json);

            try
            {
                // Tenta obter a propriedade, disparando exceção se a prorpiedade não existir.
                Newtonsoft.Json.Linq.JToken jToken = jsonObject.SelectToken(propertyPath, true);
                propertyExists = true;

                // Se a propriedade existe e seu valor não é nulo,
                // obtem o valor tipado adequadamente.
                if (jToken != null)
                {
                    if (jToken.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                        return jToken.ToObject<ExpandoObject>();
                    else if (jToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        if (jToken.HasValues)
                        {
                            if (jToken.First.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                                return jToken.ToObject<ExpandoObject[]>();
                        }

                        return jToken.ToObject<object[]>();
                    }
                    else return jToken.ToObject<dynamic>();
                }
                else return default;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // Se chegar aqui, a propriedade não existe.
                propertyExists = false;
                return default;
            }
        }

        public static bool PropertyExists(this ExpandoObject obj, string propertyName)
        {
            if (String.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentOutOfRangeException(nameof(propertyName));

            if (obj != null)
            {
                obj.GetPropertyByPath(propertyName, out bool propertyExists);
                return propertyExists;
            }

            return false;
        }
    }
}
