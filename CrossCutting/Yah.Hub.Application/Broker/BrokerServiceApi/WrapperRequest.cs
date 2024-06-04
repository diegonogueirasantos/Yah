namespace Yah.Hub.Application.Broker.BrokerServiceApi
{
    public class WrapperRequest<T>
    {
        public WrapperRequest(T data, HttpMethod method, string operationId)
        {
            Data = data;
            Method = method;
            OperationId = operationId;
        }

        public T Data { get; set; }
        public HttpMethod Method { get; set; }
        public string OperationId { get; set; }
    }
}
