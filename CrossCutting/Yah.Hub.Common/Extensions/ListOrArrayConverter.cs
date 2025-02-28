﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yah.Hub.Common.Extensions
{
    /// <summary>
    /// Permite desserializar uma propriedade de um json que pode ser uma lista ou um objeto
    /// https://stackoverflow.com/questions/18994685/how-to-handle-both-a-single-item-and-an-array-for-the-same-property-using-json-n
    /// <example>
    ///     [JsonConverter(typeof(SingleOrArrayConverter<string>))]
    ///     public List<string> Categories { get; set; }
    /// <example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (!token.HasValues)
                return new List<T>();

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }
    }
}
