using System;
using Newtonsoft.Json;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Common.ServiceMessage
{
    public class ServiceMessage : IServiceMessage
    {
        #region Constructor

        public ServiceMessage(Yah.Hub.Common.Identity.Identity identity)
        {
            this.Identity = identity;
        }

        #endregion

        #region Properties

        public virtual List<Error> Errors { get; set; } = new List<Error>();
        
        [JsonIgnore]
        public Yah.Hub.Common.Identity.Identity Identity { get; }

        public bool HasBusinessError
        {
            get
            {
                return this.Errors?.Any(x => x.Type == ErrorType.Business) ?? false;
            }
        }

        public bool IsValid
        {
            get
            {
                return !this.Errors?.Any() ?? true;
            }
        }

        #endregion

        #region Static Constructors

        public static ServiceMessage CreateValidResult(Yah.Hub.Common.Identity.Identity identity)
        {
            return new ServiceMessage(identity);
        }

        public static ServiceMessage CreateInvalidResult(Yah.Hub.Common.Identity.Identity identity, Error error)
        {
            var result = new ServiceMessage(identity);
            result.WithError(error);

            return result;
        }

        public static ServiceMessage CreateInvalidResult(Yah.Hub.Common.Identity.Identity identity, List<Error> error)
        {
            var result = new ServiceMessage(identity);
            result.WithErrors(error);

            return result;
        }

        #endregion

        #region Methods

        public void WithErrors(List<Error> errors)
        {
            if (this.Errors == null)
                this.Errors = new List<Error>();

            if (errors != null)
                this.Errors?.AddRange(errors);
        }

        public void WithError(Error error)
        {
            if (this.Errors == null)
                this.Errors = new List<Error>();

            if (error != null)
                this.Errors?.Add(error);
        }

        //private void VerifyIntegrity()
        //{
        //    Configuration.IssueConfiguration();
        //}

        #endregion
    }

    public class ServiceMessage<T> : ServiceMessage, IServiceMessage<T>
    {
        public ServiceMessage(Yah.Hub.Common.Identity.Identity identity) : base(identity)
        {
        }

        public ServiceMessage(Yah.Hub.Common.Identity.Identity identity, T data) : base(identity)
        {
            this.Data = data;
        }

        #region Properties

        public T Data { get; set; }

        #endregion


        #region Static Constructors

        public static ServiceMessage<T> CreateValidResult(Yah.Hub.Common.Identity.Identity identity, T data)
        {
            var result = new ServiceMessage<T>(identity, data);
            return result;
        }

        public static new ServiceMessage<T> CreateValidResult(Yah.Hub.Common.Identity.Identity identity)
        {
            var result = new ServiceMessage<T>(identity);
            return result;
        }

        public static ServiceMessage<T> CreateInvalidResult(Yah.Hub.Common.Identity.Identity identity, Error error, T data)
        {
            var result = new ServiceMessage<T>(identity, data);
            result.WithError(error);
            return result;
        }

        public static ServiceMessage<T> CreateInvalidResult(Yah.Hub.Common.Identity.Identity identity, List<Error> errors, T data)
        {
            var result = new ServiceMessage<T>(identity, data);
            result.WithErrors(errors);
            return result;
        }

        #endregion

        #region Methods

        public void WithData(T data)
        {
            this.Data = data;
        }

        #endregion
    }
}

