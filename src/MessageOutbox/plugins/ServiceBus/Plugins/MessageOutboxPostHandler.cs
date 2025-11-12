using Microsoft.Xrm.Sdk;
using ServiceBus.Models;
using ServiceBus.Services;
using System;

namespace ServiceBus.Plugins
{
    /// <summary>
    /// Dataverse plugin that sends messages to Azure Service Bus using managed identity authentication.
    /// This plugin runs in PostOperation on the ppcc_messageoutbox entity and sends the serialized message to Service Bus.
    /// Configuration is read from environment variables: ppcc_ServiceBusNamespace and ppcc_ServiceBusQueue
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class MessageOutboxPostHandler : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the MessageOutboxPostHandler class
        /// </summary>
        /// <param name="unsecureConfiguration">Not used - configuration is read from environment variables</param>
        /// <param name="secureConfiguration">Not used - configuration is read from environment variables</param>
        public MessageOutboxPostHandler(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(MessageOutboxPostHandler))
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
                localPluginContext.Trace($"Starting MessageOutboxPostHandler plugin execution for ppcc_messageoutbox");

                // Ensure this is running on the ppcc_messageoutbox entity
                if (context.PrimaryEntityName != ppcc_MessageOutbox.EntityLogicalName)
                {
                    localPluginContext.Trace($"Plugin registered on incorrect entity: {context.PrimaryEntityName}");
                    return;
                }

                // Get the target message outbox record
                ppcc_MessageOutbox messageOutbox = GetMessageOutboxRecord(localPluginContext, context);

                if (messageOutbox == null)
                {
                    localPluginContext.Trace("Message outbox record not found");
                    return;
                }

                localPluginContext.Trace($"Processing message outbox: {messageOutbox.ppcc_Name}");
                localPluginContext.Trace($"Entity: {messageOutbox.ppcc_Entity}, RecordId: {messageOutbox.ppcc_RecordId}");

                // Get configuration and access token using helper
                var configuration = ServiceBusHelper.GetServiceBusConfiguration(
                    localPluginContext.PluginUserService,
                    localPluginContext.TracingService);

                var accessToken = ServiceBusHelper.AcquireServiceBusAccessToken(
                    localPluginContext.ManagedIdentityService,
                    localPluginContext.TracingService);

                // Create token credential
                var credential = new PluginManagedIdentityTokenCredential(accessToken);

                // Send the serialized message to Service Bus
                string serviceBusMessageId;
                using (var serviceBusService = new ServiceBusMessageService(
                    configuration.Namespace,
                    configuration.QueueName,
                    credential,
                    localPluginContext.TracingService))
                {
                    serviceBusMessageId = serviceBusService.SendMessage(
                        messageOutbox.ppcc_Message,
                        context.CorrelationId,
                        messageOutbox.ppcc_Entity,
                        "MessageOutbox",
                        context.OrganizationId);
                }

                // Update the message outbox record with sent information using helper
                ServiceBusHelper.UpdateMessageOutboxSentInfo(
                    localPluginContext.PluginUserService,
                    localPluginContext.TracingService,
                    messageOutbox.Id,
                    serviceBusMessageId);

                localPluginContext.Trace("MessageOutboxPostHandler plugin execution completed successfully");
            }
            catch (Exception ex)
            {
                localPluginContext.Trace($"Error in MessageOutboxPostHandler plugin: {ex.Message}");
                localPluginContext.Trace($"Stack trace: {ex.StackTrace}");
                throw new InvalidPluginExecutionException($"An error occurred in the MessageOutboxPostHandler plugin: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves the message outbox record from the target or by querying
        /// </summary>
        private ppcc_MessageOutbox GetMessageOutboxRecord(ILocalPluginContext localPluginContext, IPluginExecutionContext context)
        {
            ppcc_MessageOutbox messageOutbox = null;

            // Try to get from Target in InputParameters
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity target)
            {
                messageOutbox = target.ToEntity<ppcc_MessageOutbox>();

                // If the message field is not in the target, retrieve the full record
                if (string.IsNullOrWhiteSpace(messageOutbox.ppcc_Message))
                {
                    localPluginContext.Trace($"Message field not in target, retrieving full record: {target.Id}");
                    messageOutbox = localPluginContext.PluginUserService.Retrieve(
                        ppcc_MessageOutbox.EntityLogicalName,
                        target.Id,
                        new Microsoft.Xrm.Sdk.Query.ColumnSet(
                            ppcc_MessageOutbox.Fields.ppcc_Name,
                            ppcc_MessageOutbox.Fields.ppcc_Message,
                            ppcc_MessageOutbox.Fields.ppcc_Entity,
                            ppcc_MessageOutbox.Fields.ppcc_RecordId
                        )).ToEntity<ppcc_MessageOutbox>();
                }
            }
            // Try to get from PostImage
            else if (context.PostEntityImages != null && context.PostEntityImages.Contains("PostImage"))
            {
                localPluginContext.Trace("Getting message outbox from PostImage");
                messageOutbox = context.PostEntityImages["PostImage"].ToEntity<ppcc_MessageOutbox>();
            }

            return messageOutbox;
        }
    }
}
