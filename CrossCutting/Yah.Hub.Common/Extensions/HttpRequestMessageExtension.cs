using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

namespace Yah.Hub.Common.Extensions
{
    public static class HttpRequestMessageExtension
    {
        public static void SetHeaders(this HttpRequestMessage request, Dictionary<string, string> headers)
        {
            foreach (var element in headers)
                request.Headers.Add(element.Key, element.Value);
        }

        public static void SetJsonContent<T>(this HttpRequestMessage request, T content, MediaTypeFormatter formatter)
        {
            request.Content = new ObjectContent<T>(content, formatter);
        }

        public static void SetJsonContent<T>(this HttpRequestMessage request, T content)
        {
            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
        }

        public static void SetEncodedContent(this HttpRequestMessage request, Dictionary<string, string> value)
        {
            request.Content = new FormUrlEncodedContent(value);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }

        public static void SetXMLContent(this HttpRequestMessage request, HttpContent content)
        {
            request.Content = content;
        }
    }
}