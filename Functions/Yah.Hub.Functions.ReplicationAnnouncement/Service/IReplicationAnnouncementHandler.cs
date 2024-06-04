using Amazon.DynamoDBv2.DataModel;
using Yah.Hub.Common.ServiceMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace Yah.Hub.Functions.ReplicationAnnouncement.Service
{
    public interface IReplicationAnnouncementHandler
    {
        public Task<ServiceMessage> ReplicateAnnouncement(DynamodbStreamRecord record);
    }
}
