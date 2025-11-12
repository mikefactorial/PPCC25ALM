using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceBus.Models;
using ServiceBus.Services;
using System;
using System.Collections.Generic;

namespace ServiceBus.CustomApis
{
    /// <summary>
    /// Custom API plugin that processes messages from Azure Service Bus queue in PeekLock mode.
    /// Deserializes messages, updates corresponding message outbox records, and completes messages.
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class ProcessMessages : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the ProcessMessages class
        /// </summary>
        /// <param name="unsecureConfiguration">unsecureConfiguration</param>
        /// <param name="secureConfiguration">secureConfiguration</param>
        public ProcessMessages(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(ProcessMessages))
        {
        }

        /// <summary>
        /// Entry point for custom business logic execution
        /// </summary>
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;

            try
            {
                localPluginContext.Trace("Starting ProcessMessages plugin execution");

                // Get optional input parameter
                int batchSize = GetInputParameter<int>(context, "BatchSize", 10);
                int maxWaitTimeSeconds = 10; // Fixed wait time

                localPluginContext.Trace($"BatchSize: {batchSize}, MaxWaitTime: {maxWaitTimeSeconds}s");

                // Get configuration and access token using helper
                var configuration = ServiceBusHelper.GetServiceBusConfiguration(
                    localPluginContext.PluginUserService,
                    localPluginContext.TracingService);

                var accessToken = ServiceBusHelper.AcquireServiceBusAccessToken(
                    localPluginContext.ManagedIdentityService,
                    localPluginContext.TracingService);

                // Create token credential
                var credential = new PluginManagedIdentityTokenCredential(accessToken);

                int processedCount = 0;
                int errorCount = 0;

                // Receive and process messages from Service Bus
                using (var receiverService = new ServiceBusReceiverService(
                    configuration.Namespace,
                    configuration.QueueName,
                    credential,
                    localPluginContext.TracingService))
                {
                    var messages = receiverService.ReceiveMessages(batchSize, maxWaitTimeSeconds);

                    localPluginContext.Trace($"Received {messages.Count} messages from queue");

                    foreach (var message in messages)
                    {
                        try
                        {
                            localPluginContext.Trace($"Processing message: {message.MessageId}");

                            // Get message body as string
                            var messageBody = message.Body.ToString();
                            localPluginContext.Trace($"Message body length: {messageBody.Length}");

                            // Deserialize the message to extract PrimaryEntityId
                            var executionContext = DeserializeMessage(messageBody, localPluginContext);

                            if (executionContext != null && executionContext.PrimaryEntityId != Guid.Empty)
                            {
                                // Update message outbox records
                                UpdateMessageOutboxRecords(
                                    localPluginContext,
                                    executionContext.PrimaryEntityId.ToString(),
                                    message.MessageId);

                                // Complete the message
                                receiverService.CompleteMessage(message);
                                processedCount++;

                                localPluginContext.Trace($"Message {message.MessageId} processed successfully");
                            }
                            else
                            {
                                localPluginContext.Trace($"Invalid message format or missing PrimaryEntityId");
                                receiverService.DeadLetterMessage(
                                    message,
                                    "InvalidMessageFormat",
                                    "Message does not contain valid PrimaryEntityId");
                                errorCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            localPluginContext.Trace($"Error processing message {message.MessageId}: {ex.Message}");
                            localPluginContext.Trace($"Stack trace: {ex.StackTrace}");

                            try
                            {
                                // Abandon the message so it can be retried
                                receiverService.AbandonMessage(message);
                                errorCount++;
                            }
                            catch (Exception abandonEx)
                            {
                                localPluginContext.Trace($"Error abandoning message: {abandonEx.Message}");
                            }
                        }
                    }
                }

                localPluginContext.Trace($"ProcessMessages completed. Processed: {processedCount}, Errors: {errorCount}");
            }
            catch (Exception ex)
            {
                localPluginContext.Trace($"Error in ProcessMessages plugin: {ex.Message}");
                localPluginContext.Trace($"Stack trace: {ex.StackTrace}");
                throw new InvalidPluginExecutionException($"An error occurred in the ProcessMessages plugin: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserializes the Service Bus message to extract execution context information
        /// </summary>
        private ExecutionContextInfo DeserializeMessage(string messageBody, ILocalPluginContext localPluginContext)
        {
            try
            {
                var jsonObject = JObject.Parse(messageBody);

                var primaryEntityId = jsonObject["PrimaryEntityId"]?.Value<string>();
                var primaryEntityName = jsonObject["PrimaryEntityName"]?.Value<string>();
                var messageName = jsonObject["MessageName"]?.Value<string>();

                if (string.IsNullOrWhiteSpace(primaryEntityId))
                {
                    localPluginContext.Trace("PrimaryEntityId not found in message");
                    return null;
                }

                Guid entityId;
                if (!Guid.TryParse(primaryEntityId, out entityId))
                {
                    localPluginContext.Trace($"Invalid PrimaryEntityId format: {primaryEntityId}");
                    return null;
                }

                return new ExecutionContextInfo
                {
                    PrimaryEntityId = entityId,
                    PrimaryEntityName = primaryEntityName,
                    MessageName = messageName
                };
            }
            catch (JsonException ex)
            {
                localPluginContext.Trace($"Error deserializing message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates message outbox records to mark them as processed
        /// </summary>
        private void UpdateMessageOutboxRecords(
            ILocalPluginContext localPluginContext,
            string recordId,
            string messageId)
        {
            localPluginContext.Trace($"Updating message outbox records for RecordId: {recordId}");

            // Query for message outbox records with matching RecordId
            var query = new QueryExpression(ppcc_MessageOutbox.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(ppcc_MessageOutbox.Fields.ppcc_MessageOutboxId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            ppcc_MessageOutbox.Fields.ppcc_RecordId,
                            ConditionOperator.Equal,
                            recordId),
                        new ConditionExpression(
                            ppcc_MessageOutbox.Fields.statuscode,
                            ConditionOperator.Equal,
                            (int)ppcc_messageoutbox_statuscode.Sent)
                    }
                }
            };

            var results = localPluginContext.PluginUserService.RetrieveMultiple(query);

            localPluginContext.Trace($"Found {results.Entities.Count} message outbox records to update");

            foreach (var entity in results.Entities)
            {
                ServiceBusHelper.UpdateMessageOutboxProcessedInfo(
                    localPluginContext.PluginUserService,
                    localPluginContext.TracingService,
                    entity.Id,
                    messageId);
            }
        }

        /// <summary>
        /// Gets an input parameter value with a default fallback
        /// </summary>
        private T GetInputParameter<T>(IPluginExecutionContext context, string parameterName, T defaultValue)
        {
            if (context.InputParameters.Contains(parameterName))
            {
                var value = context.InputParameters[parameterName];
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Helper class to hold deserialized execution context information
        /// </summary>
        private class ExecutionContextInfo
        {
            public Guid PrimaryEntityId { get; set; }
            public string PrimaryEntityName { get; set; }
            public string MessageName { get; set; }
        }
    }
}
