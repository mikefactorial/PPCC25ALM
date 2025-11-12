using Microsoft.Xrm.Sdk;
using ServiceBus.Models;
using ServiceBus.Services;
using System;

namespace ServiceBus.Plugins
{
    /// <summary>
    /// Dataverse plugin that creates a message outbox record in the PreOperation stage.
    /// This plugin captures the execution context and saves it to ppcc_messageoutbox for later processing.
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class CreateMessageOutbox : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the CreateMessageOutbox class
        /// </summary>
        /// <param name="unsecureConfiguration">Not used</param>
        /// <param name="secureConfiguration">Not used</param>
        public CreateMessageOutbox(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(CreateMessageOutbox))
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
                localPluginContext.Trace($"Starting CreateMessageOutbox plugin execution for entity: {context.PrimaryEntityName}, message: {context.MessageName}");

                // Get the target entity
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                {
                    localPluginContext.Trace("Target entity not found in InputParameters");
                    return;
                }

                var target = (Entity)context.InputParameters["Target"];
                var targetId = target.Id != Guid.Empty ? target.Id : context.PrimaryEntityId;
                var targetLogicalName = target.LogicalName;

                // Get the record name (if available in target or pre-image)
                string recordName = GetRecordName(target, context, localPluginContext);

                // Serialize the execution context
                var serializer = new PluginContextSerializer(localPluginContext.TracingService);
                var serializedContext = serializer.SerializeContext(context);

                // Create the message outbox record
                var messageOutbox = new ppcc_MessageOutbox
                {
                    ppcc_RecordId = targetId.ToString(),
                    ppcc_Name = $"{recordName} - {context.MessageName}",
                    ppcc_Message = serializedContext,
                    ppcc_Entity = targetLogicalName
                };

                localPluginContext.Trace($"Creating message outbox record for {targetLogicalName} ({targetId})");

                var outboxId = localPluginContext.PluginUserService.Create(messageOutbox);

                localPluginContext.Trace($"Message outbox record created successfully with ID: {outboxId}");
            }
            catch (Exception ex)
            {
                localPluginContext.Trace($"Error in CreateMessageOutbox plugin: {ex.Message}");
                localPluginContext.Trace($"Stack trace: {ex.StackTrace}");
                throw new InvalidPluginExecutionException($"An error occurred in the CreateMessageOutbox plugin: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Attempts to get the record name from the target entity or pre-image
        /// </summary>
        private string GetRecordName(Entity target, IPluginExecutionContext context, ILocalPluginContext localPluginContext)
        {
            string recordName = "Unknown";

            // Try to get name from common name attributes
            string[] nameAttributes = { "name", "fullname", "subject", "title" };

            foreach (var attr in nameAttributes)
            {
                if (target.Contains(attr))
                {
                    recordName = target.GetAttributeValue<string>(attr);
                    if (!string.IsNullOrWhiteSpace(recordName))
                    {
                        localPluginContext.Trace($"Record name found in target: {recordName}");
                        return recordName;
                    }
                }
            }

            // If not in target, try pre-image
            if (context.PreEntityImages != null && context.PreEntityImages.Contains("PreImage"))
            {
                var preImage = context.PreEntityImages["PreImage"];
                foreach (var attr in nameAttributes)
                {
                    if (preImage.Contains(attr))
                    {
                        recordName = preImage.GetAttributeValue<string>(attr);
                        if (!string.IsNullOrWhiteSpace(recordName))
                        {
                            localPluginContext.Trace($"Record name found in pre-image: {recordName}");
                            return recordName;
                        }
                    }
                }
            }

            localPluginContext.Trace($"No record name found, using default: {recordName}");
            return recordName;
        }
    }
}
