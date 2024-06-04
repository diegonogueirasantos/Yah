namespace Yah.Hub.Domain.Monitor
{
    public interface RequestState<T>
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string IntegrationId { get; set; }
        public T Data { get; set; }
    }
}
