using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SignalRAzureFunction;

public static class DbChangeTriggered
{
    private static int _totalCount;

    [FunctionName("Negotiate")]
    public static SignalRConnectionInfo GetSignalRConnectionInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequest request,
        [SignalRConnectionInfo(HubName = "Weather")]
        SignalRConnectionInfo connectionInfo)
    {
        return connectionInfo;
    }

    [FunctionName("DbChangeTriggered")]
    public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "SignalR",
            collectionName: "SignalR",
            ConnectionStringSetting = "CosmosDbConnectionString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]
        IReadOnlyList<Document> documents,
        [SignalR(HubName = "Weather")] IAsyncCollector<SignalRMessage> signalRMessages,
        ExecutionContext context,
        ILogger log)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var environment = config["Environment"];

        log.LogInformation("Environment:  " + environment);
        if (documents != null && documents.Count > 0)
        {
            _totalCount++;
            log.LogInformation("Number of documents modifications this session:  " + _totalCount);
            
            for (var docNumber = 0; docNumber < documents.Count; docNumber++)
            {
                log.LogInformation("Modified document Id:  " + documents[docNumber].Id);

                var message = documents.Select(doc => new
                    {
                        id = documents[docNumber].Id,
                        date = doc.GetPropertyValue<string>("Date"),
                        temperatureC = doc.GetPropertyValue<string>("TemperatureC"),
                        temperatureF = doc.GetPropertyValue<string>("TemperatureF"),
                        summary = doc.GetPropertyValue<string>("Summary"),
                    })
                    .ToList();

                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "newMessage",
                        Arguments = new object[] { message }
                    });
            }
        }
    }
}
