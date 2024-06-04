using System.Security.Claims;

namespace Yah.Hub.Common.Security.Identities
{
    public class WorkerIdentity : Identity.Identity
    {
        public const string Actor = "ClaimType";
        public const string WorkerActor = "worker";

        public WorkerIdentity()
        : base(new Dictionary<string, string>() {
                { Actor, WorkerActor }
                })
        { }
    }
}
