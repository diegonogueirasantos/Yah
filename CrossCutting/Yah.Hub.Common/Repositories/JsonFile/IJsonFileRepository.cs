using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Common.Repositories.JsonFile
{
    public interface IJsonFileRepository<T>
    {
        Task<T> GetAsync<T>(string fileName);
    }
}
