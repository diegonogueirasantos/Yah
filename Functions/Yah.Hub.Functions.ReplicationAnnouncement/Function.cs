using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yah.Hub.Functions.ReplicationAnnouncement.Service;
using Newtonsoft.Json;
using Yah.Hub.Function.Bootstrap;
using Microsoft.Extensions.Logging;
using Nest;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Security;

namespace Yah.Hub.Functions.ReplicationAnnouncement;
public class Function
{
    public async Task Handler(DynamoDBEvent input, FunctionEnvironment environment)
    {
        var logger = environment.ServiceProvider.GetService<ILogger<Function>>();
        var requestId = Guid.NewGuid().ToString();
        
        try
        {
            logger.LogInformation($"Start Function:Handle:{requestId} ({JsonConvert.SerializeObject(input)})");
            input = input ?? throw new ArgumentNullException(nameof(input));
            environment = environment ?? throw new ArgumentNullException(nameof(environment));

            var handle = environment.ServiceProvider.GetService<IReplicationAnnouncementHandler>();

            foreach (var record in input.Records)
            {
                await handle.ReplicateAnnouncement(record);
            }
        }
        catch (Exception ex)
        {
            Error error = new Error(ex);
            logger.LogCritical("Error while replicate announcement",ex);
            throw ex;
        }
    }

    private static FunctionEnvironment Environment { get; }

    static Function()
            => Environment = FunctionEnvironmentHelper.Build();

    private static async Task Main(string[] args)
    {
        Action<DynamoDBEvent> func = FunctionHandler;
        using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func, new Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer()))
        using (var bootstrap = new LambdaBootstrap(handlerWrapper))
            await bootstrap.RunAsync();
    }

    public static void FunctionHandler(DynamoDBEvent input)
    => new Function().Handler(input, Environment).ConfigureAwait(false).GetAwaiter().GetResult();
}