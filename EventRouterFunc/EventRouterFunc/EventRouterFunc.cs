using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace EventRouterFunc
{
    public class EventRouterFunc
    {
        const string storageQueueSourceQueue = "eventbus";
        const string storageQueueSourceConnection = "storageQueueSourceConnection";
        const string storageQueueDestinationConnection = "storageQueueDestinationConnection";

        private readonly TelemetryClient telemetryClient;
        public EventRouterFunc(TelemetryConfiguration telemetryConfiguration)
        {
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("EventRouterQ2Q")]
        public async Task RunQueueDataProcessor([QueueTrigger(storageQueueSourceQueue, Connection = storageQueueSourceConnection)] CloudQueueMessage queueMessage,
            ExecutionContext executionContext, ILogger log)
        {
            string eventTime, api, url;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            string messageBody = queueMessage.AsString;

            // Parse message
            // "eventTime": "2021-02-19T22:29:30.8224549Z"
            // "api": "CreateFile"
            // "url": "https://attautoloaderqueuepoc02.dfs.core.windows.net/fs1/table/file0010000002.txt"
            try
            {
                JsonElement root, data;
                using JsonDocument doc = JsonDocument.Parse(messageBody);
                root = doc.RootElement;
                eventTime = root.GetProperty("eventTime").ToString();
                data = root.GetProperty("data");
                api = data.GetProperty("api").ToString();
                url = data.GetProperty("url").ToString();
            }
            catch (Exception ex)
            {
                log.LogInformation($"UnableToProcessMesasageBody. ex='{ex.Message}");
                throw ex;
            }

            string destinationQueueName;
            try
            {
                destinationQueueName = HashFunction.GetQueueName(url); ;
            }
            catch (Exception ex)
            {
                log.LogInformation($"UnableToObtainQueueNameFromHash. ex='{ex.Message}");
                throw ex;
            }


            try
            {
                var connectionString = Environment.GetEnvironmentVariable(storageQueueDestinationConnection);
                QueueClient queueClient = new QueueClient(connectionString, destinationQueueName);

                bool queueExists  = await Task.Run(() => queueClient.ExistsAsync());
                // Create the queue
                if (queueExists)
                {
                    SendReceipt sendReceipt = await Task.Run(async () => await queueClient.SendMessageAsync(messageBody));
                    log.LogInformation($"erresult=0,f={url},q={destinationQueueName}");
                }
                else
                {
                    QueueClient queueClientDeadLetter = new QueueClient(connectionString, "mydeadletter");
                    await queueClientDeadLetter.CreateIfNotExistsAsync();
                    await queueClientDeadLetter.SendMessageAsync(messageBody);
                }

            }
            catch (Exception ex)
            {
                log.LogInformation($"UnableToMoveMessage to queue {destinationQueueName}. ex='{ex.Message}'");
                throw ex;
            }

            stopwatch.Stop();

            try
            {
                var metrics = new Dictionary<string, double> { { "processingTime", stopwatch.Elapsed.TotalMilliseconds } };
                var properties = new Dictionary<string, string> { { "tableName", "<tableName>" } };
                this.telemetryClient.TrackEvent("EventProcessed", properties, metrics);

            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}


//private async Task CreateQueueAndSendMessage(string queueName, string messageBody, ILogger log)
//{
//    try
//    {
//        var connectionString = Environment.GetEnvironmentVariable(storageQueueDestinationConnection);
//        QueueClient queueClient = new QueueClient(connectionString, queueName);

//        // Create the queue
//        await queueClient.CreateIfNotExistsAsync();
//        await queueClient.SendMessageAsync(messageBody);
//    }
//    catch (Exception ex)
//    {
//        log.LogError($"An error occurred while writing to queue={queueName} Error={ex.Message}");
//        throw ex;
//    }
//    return;
//}

