using CsvHelper;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using System.Net.Http.Formatting;
using System.Xml.Serialization;
using System.Linq;

namespace Yah.Hub.Common.Extensions
{
    public static class HttpExtensions
    {
        public async static Task<ServiceMessage<T>> TryReadAsAsync<T>(this HttpResponseMessage response, MarketplaceServiceMessage message, params MediaTypeFormatter[] mediaTypeFormatter)
            where T : class
        {
            T model = null;

            try
            {
                model = await response.Content.ReadAsAsync<T>(mediaTypeFormatter);
            }
            catch (Exception e)
            {
                return ServiceMessage<T>.CreateInvalidResult(message.Identity, new Error("Falha na leitura dos dados", "Falha na leitura dos dados", ErrorType.Business), null);
            }

            return ServiceMessage<T>.CreateValidResult(message.Identity, model);
        }

        public async static Task<ServiceMessage<T>> TryReadAsAsync<T>(this ServiceMessage<HttpResponseMessage> message)
            where T : class
        {
            T model = null;

            try
            {
                model = await message.Data.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception e)
            {
                return ServiceMessage<T>.CreateInvalidResult(message.Identity, new Error("Falha na leitura dos dados", "Falha na leitura dos dados", ErrorType.Business), null);
            }

            return ServiceMessage<T>.CreateValidResult(message.Identity, model);
        }

        public async static Task<ServiceMessage<T>> TryReadAsAsync<T>(this HttpResponseMessage response, MarketplaceServiceMessage message)
             where T : class
        {
            T model = null; 

            try
            {
                model = await response.Content.ReadAsAsync<T>();
            }
            catch (Exception e)
            {
                return ServiceMessage<T>.CreateInvalidResult(message.Identity, new Error("Falha na leitura dos dados", "Falha na leitura dos dados", ErrorType.Business), null);
            }

            return ServiceMessage<T>.CreateValidResult(message.Identity, model);
        }

        public async static Task<ServiceMessage<T>> TryReadXmlAsAsync<T>(this HttpResponseMessage message, ServiceMessage.ServiceMessage serviceMessage)
            where T : class
        {
            T model = null;

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                using (Stream s = await message.Content.ReadAsStreamAsync())
                {
                    model = (T)xmlSerializer.Deserialize(s);
                }
            }
            catch (Exception e)
            {
                return ServiceMessage<T>.CreateInvalidResult(serviceMessage.Identity, new Error("Falha na leitura dos dados do XML", "Falha na leitura dos dados do XML", ErrorType.Business), null);
            }

            return ServiceMessage<T>.CreateValidResult(serviceMessage.Identity, model);
        }

        public async static Task<ServiceMessage<List<T>>> TryReadCsvAsAsync<T>(this HttpResponseMessage message, ServiceMessage.ServiceMessage serviceMessage)
            where T : class
        {
            List<T> model = null;

            try
            {
                using (Stream s = await message.Content.ReadAsStreamAsync())
                {

                    using(TextReader reader = new StreamReader(s))
                    {
                        var csvReader = new CsvReader(reader);
                        csvReader.Configuration.IgnoreReferences = true;
                        csvReader.Configuration.Delimiter = ";";

                        var records = csvReader.GetRecords<T>()?.ToList();

                        if ((records?.Count() ?? 0) == 0)
                        {
                            return ServiceMessage<List<T>>.CreateInvalidResult(serviceMessage.Identity, new Error("Falha na leitura dos dados do csv", "Falha na leitura dos dados do csv", ErrorType.Business), null);
                        }

                        model = records;
                    }
                }
            }
            catch (Exception e)
            {
                return ServiceMessage<List<T>>.CreateInvalidResult(serviceMessage.Identity, new Error("Falha na leitura dos dados", "Falha na leitura dos dados", ErrorType.Business), null);
            }

            return ServiceMessage<List<T>>.CreateValidResult(serviceMessage.Identity, model);
        }
    }
}
