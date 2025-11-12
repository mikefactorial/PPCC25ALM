using Microsoft.Xrm.Sdk;
using ServiceBus.Models;
using System;

namespace ServiceBus.Services
{
    /// <summary>
    /// Helper class containing shared Service Bus logic for plugins
    /// </summary>
    public static class ServiceBusHelper
    {
        /// <summary>
        /// Retrieves Service Bus configuration from environment variables
        /// </summary>
        public static ServiceBusConfiguration GetServiceBusConfiguration(
            IOrganizationService organizationService,
            ITracingService tracingService)
        {
            tracingService?.Trace("Retrieving Service Bus configuration from environment variables");

            var envVarService = new EnvironmentVariableService(organizationService, tracingService);

            var serviceBusNamespace = envVarService.GetValue("ppcc_ServiceBusNamespace");
            var queueName = envVarService.GetValue("ppcc_ServiceBusQueue");

            if (string.IsNullOrWhiteSpace(serviceBusNamespace))
            {
                throw new InvalidPluginExecutionException("Environment variable 'ppcc_ServiceBusNamespace' is not set or is empty");
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidPluginExecutionException("Environment variable 'ppcc_ServiceBusQueue' is not set or is empty");
            }

            tracingService?.Trace($"Configuration retrieved - Namespace: {serviceBusNamespace}, Queue: {queueName}");

            return new ServiceBusConfiguration
            {
                Namespace = serviceBusNamespace,
                QueueName = queueName
            };
        }

        /// <summary>
        /// Acquires an access token for Service Bus using managed identity
        /// </summary>
        public static string AcquireServiceBusAccessToken(
            IManagedIdentityService managedIdentityService,
            ITracingService tracingService)
        {
            tracingService?.Trace("Acquiring access token for Service Bus");

            if (managedIdentityService == null)
            {
                throw new InvalidPluginExecutionException("IManagedIdentityService is not available in the plugin context");
            }

            var scopes = new[] { "https://servicebus.azure.net/.default" };

            var accessToken = managedIdentityService.AcquireToken(scopes);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidPluginExecutionException("Failed to acquire access token from managed identity service");
            }

            tracingService?.Trace("Access token acquired successfully");
            return accessToken;
        }

        /// <summary>
        /// Updates the message outbox record with sent information
        /// </summary>
        public static void UpdateMessageOutboxSentInfo(
            IOrganizationService organizationService,
            ITracingService tracingService,
            Guid messageOutboxId,
            string serviceBusMessageId)
        {
            try
            {
                tracingService?.Trace($"Updating message outbox record {messageOutboxId} with sent information");

                var updateEntity = new ppcc_MessageOutbox
                {
                    Id = messageOutboxId,
                    ppcc_MessageId = serviceBusMessageId,
                    ppcc_MessageSentOn = DateTime.UtcNow,
                    statuscode = ppcc_messageoutbox_statuscode.Sent
                };

                organizationService.Update(updateEntity);

                tracingService?.Trace("Message outbox record updated successfully");
            }
            catch (Exception ex)
            {
                tracingService?.Trace($"Error updating message outbox record: {ex.Message}");
                // Don't throw - message was already sent successfully
            }
        }

        /// <summary>
        /// Updates the message outbox record with processed information
        /// </summary>
        public static void UpdateMessageOutboxProcessedInfo(
            IOrganizationService organizationService,
            ITracingService tracingService,
            Guid messageOutboxId,
            string serviceBusMessageId)
        {
            try
            {
                tracingService?.Trace($"Updating message outbox record {messageOutboxId} with processed information");

                var updateEntity = new ppcc_MessageOutbox
                {
                    Id = messageOutboxId,
                    ppcc_MessageId = serviceBusMessageId,
                    ppcc_MessageProcessedOn = DateTime.UtcNow,
                    statuscode = ppcc_messageoutbox_statuscode.Processed
                };

                organizationService.Update(updateEntity);

                tracingService?.Trace("Message outbox record updated successfully");
            }
            catch (Exception ex)
            {
                tracingService?.Trace($"Error updating message outbox record: {ex.Message}");
                // Continue - don't fail the entire operation
            }
        }
    }
}
