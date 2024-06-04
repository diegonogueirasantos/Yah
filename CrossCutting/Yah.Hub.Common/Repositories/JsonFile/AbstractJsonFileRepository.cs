using Elasticsearch.Net;
using Newtonsoft.Json;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Services;
using System.Reflection;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Yah.Hub.Common.Repositories.JsonFile
{
    public abstract class AbstractJsonFileRepository : AbstractService
    {
        protected IHostingEnvironment HostingEnvironment { get; }
        private IDictionary<string, object> Model { get; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        protected AbstractJsonFileRepository(
            IConfiguration configuration, 
            ILogger logger,
            IHostingEnvironment hostingEnvironment) : base(configuration, logger)
        {
            HostingEnvironment = hostingEnvironment;
        }

        public async virtual Task<T> GetAsync<T>(string fileName)
        {
            T fileContent = default;
            if (this.Model.ContainsKey(fileName))
                fileContent = (T)this.Model[fileName];
            else
            {
                fileContent = await this.ReadJson<T>(fileName);
                this.Model[fileName] = fileContent;
            }

            return (T)(fileContent);
        }

        protected async virtual Task<T> ReadJson<T>(string file)
        {
            var path = this.HostingEnvironment.ContentRootPath;

            Char[] buffer;
            string json = string.Empty;

            var filepath = $"{path}\\{file}";

            if (!System.IO.File.Exists(filepath))
            {
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                filepath = $"{path}\\{file}";
            }

            var serlializer = new JsonSerializer();

            using (var sr = new StreamReader(new FileStream(filepath, FileMode.Open)))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var result = serlializer.Deserialize<T>(jsonTextReader);
                    return result;
                }
            }

            var model = JsonConvert.DeserializeObject<T>(json, CreateSerializerSettings());

            return model;
        }

        protected virtual JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };


            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return settings;
        }
    }
}
