using System;
namespace Yah.Hub.Common.ServiceMessage.Interfaces
{
    public interface IServiceMessage
    {
        public void WithErrors(List<Error> errors);
        public void WithError(Error errors);
        public Yah.Hub.Common.Identity.Identity Identity { get; }

        public List<Error> Errors { get; set; }

        public bool IsValid { get; }
        
    }

    public interface IServiceMessage<T> : IServiceMessage
    {
        public void WithData(T data);
        public T Data { get; set; }
    }
}

