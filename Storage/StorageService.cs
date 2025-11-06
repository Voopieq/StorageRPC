namespace Servers;

using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NLog;
using Newtonsoft.Json;

using Services;

public class StorageService
{
    /// <summary>
    /// Name of the request exchange.
    /// </summary>
    private static readonly String ExchangeName = "Strorage.Exchange";

    /// <summary>
    /// Name of the request queue.
    /// </summary>
    private static readonly String ServerQueueName = "Storage.StorageService";

    /// <summary>
    /// Logger for this class.
    /// </summary>
    private Logger _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Connection to RabbitMQ message broker.
    /// </summary>
    private IConnection rmqConn;

    /// <summary>
    /// Communications channel to RabbitMQ message broker.
    /// </summary>
    private IModel rmqChann;

    /// <summary>
    ///  Service logic
    /// </summary>
    private StorageLogic _storageLogic = new StorageLogic();

    /// <summary>
    ///  Constructor
    /// </summary>
    public StorageService()
    {
        // Connect to the RabbitMQ message broker
        ConnectionFactory rmqConnFact = new ConnectionFactory();
        rmqConn = rmqConnFact.CreateConnection();

        // Get channel, configure exhcanges and request queue
        rmqChann = rmqConn.CreateModel();

        rmqChann.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct);
        rmqChann.QueueDeclare(queue: ServerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        rmqChann.QueueBind(queue: ServerQueueName, exchange: ExchangeName, routingKey: ServerQueueName, arguments: null);

        // Connect to the queue as consumer
        //XXX: see https://www.rabbitmq.com/dotnet-api-guide.html#concurrency for threading issues
        EventingBasicConsumer rmqConsumer = new EventingBasicConsumer(rmqChann);
        rmqConsumer.Received += (consumer, delivery) => OnMessageReceived(((EventingBasicConsumer)consumer).Model, delivery);
        rmqChann.BasicConsume(queue: ServerQueueName, autoAck: true, consumer: rmqConsumer);
    }

    /// <summary>
    /// Is invoked to process messages received.
    /// </summary>
    /// <param name="channel">Related communications channel.</param>
    /// <param name="msgIn">Message deliver data.</param>
    private void OnMessageReceived(IModel channel, BasicDeliverEventArgs msgIn)
    {
        try
        {
            // Get call request
            var request = JsonConvert.DeserializeObject<RPCMessage>(Encoding.UTF8.GetString(msgIn.Body.ToArray()));

            // Set response as undefined by default
            RPCMessage response = null;

            switch (request.Action)
            {
                case $"Call_{nameof(_storageLogic.AddToCleanersList)}":
                    {
                        // Deserialize arguments
                        var cleanerData = JsonConvert.DeserializeObject<CleanerData>(request.Data);

                        // Make the call
                        var result = _storageLogic.AddToCleanersList(cleanerData);

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.AddToCleanersList)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }

                case $"Call_{nameof(_storageLogic.GetCleanerState)}":
                    {
                        // Deserialize arguments
                        var args = JsonConvert.DeserializeAnonymousType(request.Data, new { cleanerID = "1" });

                        // Make the call
                        var result = _storageLogic.GetCleanerState(args.cleanerID);

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.GetCleanerState)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }

                case $"Call_{nameof(_storageLogic.GetFileCount)}":
                    {
                        // Make the call
                        var result = _storageLogic.GetFileCount();

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.GetFileCount)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }

                case $"Call_{nameof(_storageLogic.IsCleaningMode)}":
                    {
                        // Make the call
                        var result = _storageLogic.IsCleaningMode();

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.IsCleaningMode)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }

                case $"Call_{nameof(_storageLogic.TrySendFile)}":
                    {
                        // Deserialize arguments
                        var fileDesc = JsonConvert.DeserializeObject<FileDesc>(request.Data);

                        // Make the call
                        var result = _storageLogic.TrySendFile(fileDesc);

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.TrySendFile)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }


                case $"Call_{nameof(_storageLogic.TryGetFile)}":
                    {
                        // Deserialize arguments
                        var args = JsonConvert.DeserializeAnonymousType(request.Data, new { fileIdx = 1 });

                        // Make the call
                        var result = _storageLogic.TryGetFile(args.fileIdx);

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.TryGetFile)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }

                case $"Call_{nameof(_storageLogic.TryRemoveOldestFile)}":
                    {
                        // Deserialize arguments
                        var args = JsonConvert.DeserializeAnonymousType(request.Data, new { cleanerID = 1 });

                        // Make the call
                        var result = _storageLogic.TryGetFile(args.cleanerID);

                        // Create response
                        response = new RPCMessage()
                        {
                            Action = $"Result_{nameof(_storageLogic.TryGetFile)}",
                            Data = JsonConvert.SerializeObject(new { Value = result })
                        };

                        break;
                    }

                default:
                    {
                        _log.Info($"Unsupported type of RPC action '{request.Action}'. Ignoring the message.");
                        break;
                    }
            }

            // Response is defined? Send reply message
            if (response != null)
            {
                // Prepare metadata for outgoing message
                var msgOutProps = channel.CreateBasicProperties();
                msgOutProps.CorrelationId = msgIn.BasicProperties.CorrelationId;

                // Send reply message to the client queue
                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: msgIn.BasicProperties.ReplyTo,
                    basicProperties: msgOutProps,
                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))
                );
            }
        }
        catch (Exception e)
        {
            _log.Error(e, "Unhandled exception caught when processing a message. The message is now lost.");
        }
    }
}