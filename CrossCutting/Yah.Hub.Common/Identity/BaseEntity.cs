using System;
namespace Yah.Hub.Common.Identity
{
	public interface IBaseEntity
	{
        public string Id { get; set; }
	}

    public class BaseEntity : IBaseEntity
    {
        public string Id { get; set; }
    }
}

