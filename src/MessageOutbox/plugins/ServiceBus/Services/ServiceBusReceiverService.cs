using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ServiceBus.Services
{
    /// <summary>
    /// Service for receiving messages from Azure Service Bus in PeekLock mode
    /// </summary>
    public class ServiceBusReceiverService : IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusReceiver _receiver;
        private readonly ITracingService _tracingService;

        /// <summary>
        /// Initializes a new instance of the ServiceBusReceiverService class
        /// </summary>
        /// <param name="serviceBusNamespace">The Service Bus namespace (e.g., "myservicebus.servicebus.windows.net")</param>
        /// <param name="queueName">The name of the queue to receive from</param>
        /// <param name="credential">Token credential for authentication</param>
        /// <param name="tracingService">Optional tracing service for logging</param>
        public ServiceBusReceiverService(
            string serviceBusNamespace,
            string queueName,
            TokenCredential credential,
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

            try
            {
                Trace($"Initializing Service Bus receiver for namespace: {serviceBusNamespace}, queue: {queueName}");

                var fullyQualifiedNamespace = serviceBusNamespace.EndsWith(".servicebus.windows.net")
                    ? serviceBusNamespace
                    : $"{serviceBusNamespace}.servicebus.windows.net";

                _client = new ServiceBusClient(fullyQualifiedNamespace, credential);

                // Create receiver in PeekLock mode (default)
                _receiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });

                Trace("Service Bus receiver initialized successfully");
            }
            catch (Exception ex)
            {
                Trace($"Error initializing Service Bus receiver: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to initialize Service Bus receiver: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Receives messages from the queue in PeekLock mode
        /// </summary>
        /// <param name="maxMessages">Maximum number of messages to receive</param>
        /// <param name="maxWaitTimeSeconds">Maximum time to wait for messages in seconds</param>
        /// <returns>List of received messages</returns>
        public List<ServiceBusReceivedMessage> ReceiveMessages(int maxMessages = 10, int maxWaitTimeSeconds = 10)
        {
            try
            {
                Trace($"Receiving up to {maxMessages} messages with {maxWaitTimeSeconds}s timeout");

                var messages = new List<ServiceBusReceivedMessage>();
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(maxWaitTimeSeconds));

                // Receive messages synchronously
                var receivedMessages = _receiver.ReceiveMessagesAsync(
                    maxMessages,
                    TimeSpan.FromSeconds(maxWaitTimeSeconds),
                    cancellationTokenSource.Token).GetAwaiter().GetResult();

                if (receivedMessages != null)
                {
                    messages.AddRange(receivedMessages);
                }

                Trace($"Received {messages.Count} messages from the queue");
                return messages;
            }
            catch (Exception ex)
            {
                Trace($"Error receiving messages: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to receive messages from Service Bus: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Completes a message, removing it from the queue
        /// </summary>
        /// <param name="message">The message to complete</param>
        public void CompleteMessage(ServiceBusReceivedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                Trace($"Completing message: {message.MessageId}");
                _receiver.CompleteMessageAsync(message).GetAwaiter().GetResult();
                Trace($"Message completed successfully: {message.MessageId}");
            }
            catch (Exception ex)
            {
                Trace($"Error completing message {message.MessageId}: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to complete message {message.MessageId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Abandons a message, returning it to the queue
        /// </summary>
        /// <param name="message">The message to abandon</param>
        public void AbandonMessage(ServiceBusReceivedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                Trace($"Abandoning message: {message.MessageId}");
                _receiver.AbandonMessageAsync(message).GetAwaiter().GetResult();
                Trace($"Message abandoned successfully: {message.MessageId}");
            }
            catch (Exception ex)
            {
                Trace($"Error abandoning message {message.MessageId}: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to abandon message {message.MessageId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Moves a message to the dead-letter queue
        /// </summary>
        /// <param name="message">The message to dead-letter</param>
        /// <param name="reason">Reason for dead-lettering</param>
        /// <param name="errorDescription">Error description</param>
        public void DeadLetterMessage(ServiceBusReceivedMessage message, string reason, string errorDescription)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                Trace($"Dead-lettering message: {message.MessageId}, Reason: {reason}");
                _receiver.DeadLetterMessageAsync(message, reason, errorDescription).GetAwaiter().GetResult();
                Trace($"Message dead-lettered successfully: {message.MessageId}");
            }
            catch (Exception ex)
            {
                Trace($"Error dead-lettering message {message.MessageId}: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to dead-letter message {message.MessageId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes a trace message if tracing service is available
        /// </summary>
        private void Trace(string message)
        {
            _tracingService?.Trace($"[ServiceBusReceiverService] {message}");
        }

        /// <summary>
        /// Disposes the Service Bus client and receiver
        /// </summary>
        public void Dispose()
        {
            try
            {
                _receiver?.DisposeAsync().GetAwaiter().GetResult();
                _client?.DisposeAsync().GetAwaiter().GetResult();
                Trace("Service Bus receiver disposed");
            }
            catch (Exception ex)
            {
                Trace($"Error disposing Service Bus receiver: {ex.Message}");
            }
        }
    }
}
