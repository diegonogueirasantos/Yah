namespace Yah.Hub.Domain.Monitor
{
    public class RequestProductState : RequestState<List<SkuIntegrationInfo>>
    {
        public RequestProductState(string id, string referenceId, string integrationId, List<SkuIntegrationInfo> data)
        {
            Id = id;
            ReferenceId = referenceId;
            IntegrationId = integrationId;
            Data = data;
        }

        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string IntegrationId { get; set; }
        public List<SkuIntegrationInfo> Data { get; set; }
    }
}
