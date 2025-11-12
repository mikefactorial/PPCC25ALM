using Azure.Messaging.ServiceBus;
using Microsoft.Xrm.Sdk;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.Services
{
    /// <summary>
    /// Service responsible for sending messages to Azure Service Bus
    /// </summary>
    public class ServiceBusMessageService : IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ITracingService _tracingService;

        /// <summary>
        /// Initializes a new instance of the ServiceBusMessageService class
        /// </summary>
        /// <param name="serviceBusNamespace">The fully qualified Service Bus namespace (e.g., myservicebus.servicebus.windows.net)</param>
        /// <param name="queueName">The name of the Service Bus queue</param>
        /// <param name="credential">The token credential for authentication</param>
        /// <param name="tracingService">Optional tracing service for logging</param>
        public ServiceBusMessageService(
            string serviceBusNamespace,
            string queueName,
            PluginManagedIdentityTokenCredential credential,
            ITracingService tracingService = null)
        {
            if (string.IsNullOrWhiteSpace(serviceBusNamespace))
            {
                throw new ArgumentNullException(nameof(serviceBusNamespace));
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            _tracingService = tracingService;
            _client = new ServiceBusClient(serviceBusNamespace, credential);
            _sender = _client.CreateSender(queueName);

            Trace($"ServiceBus client connected to namespace: {serviceBusNamespace}, queue: {queueName}");
        }

        /// <summary>
        /// Sends a JSON message to the Service Bus queue
        /// </summary>
        /// <param name="jsonMessage">The JSON string to send</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        /// <param name="entityName">Optional entity name for message properties</param>
        /// <param name="messageName">Optional message name for message properties</param>
        /// <param name="organizationId">Optional organization ID for message properties</param>
        /// <returns>The message ID of the sent message</returns>
        public string SendMessage(
            string jsonMessage,
            Guid correlationId,
            string entityName = null,
            string messageName = null,
            Guid? organizationId = null)
        {
            if (string.IsNullOrWhiteSpace(jsonMessage))
            {
                throw new ArgumentNullException(nameof(jsonMessage));
            }

            try
            {
                Trace($"Preparing to send message, size: {jsonMessage.Length} characters");

                var messageId = Guid.NewGuid().ToString();
                var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
                {
                    ContentType = "application/json",
                    MessageId = messageId,
                    CorrelationId = correlationId.ToString()
                };

                // Add custom application properties if provided
                if (!string.IsNullOrWhiteSpace(entityName))
                {
                    serviceBusMessage.ApplicationProperties.Add("EntityName", entityName);
                }

                if (!string.IsNullOrWhiteSpace(messageName))
                {
                    serviceBusMessage.ApplicationProperties.Add("MessageName", messageName);
                }

                if (organizationId.HasValue)
                {
                    serviceBusMessage.ApplicationProperties.Add("OrganizationId", organizationId.Value.ToString());
                }

                Trace($"Sending message to Service Bus with MessageId: {messageId}");

                // Send the message synchronously (plugins require synchronous execution)
                _sender.SendMessageAsync(serviceBusMessage).GetAwaiter().GetResult();

                Trace("Message sent successfully");
                return messageId;
            }
            catch (Exception ex)
            {
                Trace($"Error sending message to Service Bus: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to send message to Service Bus: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes a trace message if tracing service is available
        /// </summary>
        private void Trace(string message)
        {
            _tracingService?.Trace($"[ServiceBusMessageService] {message}");
        }

        /// <summary>
        /// Disposes of the Service Bus client and sender
        /// </summary>
        public void Dispose()
        {
            try
            {
                _sender?.DisposeAsync().GetAwaiter().GetResult();
                _client?.DisposeAsync().GetAwaiter().GetResult();
                Trace("Service Bus resources disposed");
            }
            catch (Exception ex)
            {
                Trace($"Error disposing Service Bus resources: {ex.Message}");
            }
        }
    }
}
