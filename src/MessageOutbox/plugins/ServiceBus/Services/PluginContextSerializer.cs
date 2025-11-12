using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;

namespace ServiceBus.Services
{
    /// <summary>
    /// Service responsible for serializing plugin execution context to JSON
    /// </summary>
    public class PluginContextSerializer
    {
        private readonly ITracingService _tracingService;

        /// <summary>
        /// Initializes a new instance of the PluginContextSerializer class
        /// </summary>
        /// <param name="tracingService">Optional tracing service for logging</param>
        public PluginContextSerializer(ITracingService tracingService = null)
        {
            _tracingService = tracingService;
        }

        /// <summary>
        /// Serializes the plugin execution context to JSON
        /// </summary>
        /// <param name="context">The plugin execution context to serialize</param>
        /// <returns>JSON string representation of the context</returns>
        public string SerializeContext(IPluginExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                Trace("Serializing plugin execution context");

                var contextData = new
                {
                    MessageName = context.MessageName,
                    PrimaryEntityName = context.PrimaryEntityName,
                    PrimaryEntityId = context.PrimaryEntityId,
                    UserId = context.UserId,
                    InitiatingUserId = context.InitiatingUserId,
                    OrganizationId = context.OrganizationId,
                    OrganizationName = context.OrganizationName,
                    BusinessUnitId = context.BusinessUnitId,
                    CorrelationId = context.CorrelationId,
                    OperationId = context.OperationId,
                    OperationCreatedOn = context.OperationCreatedOn,
                    Depth = context.Depth,
                    IsExecutingOffline = context.IsExecutingOffline,
                    IsInTransaction = context.IsInTransaction,
                    IsolationMode = context.IsolationMode,
                    Mode = context.Mode,
                    Stage = context.Stage,
                    InputParameters = context.InputParameters,
                    OutputParameters = context.OutputParameters,
                    PreEntityImages = context.PreEntityImages,
                    PostEntityImages = context.PostEntityImages,
                    SharedVariables = context.SharedVariables
                };

                var jsonMessage = JsonConvert.SerializeObject(contextData, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.None
                });

                Trace($"Context serialized successfully, size: {jsonMessage.Length} characters");

                return jsonMessage;
            }
            catch (Exception ex)
            {
                Trace($"Error serializing plugin context: {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to serialize plugin execution context: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes a trace message if tracing service is available
        /// </summary>
        private void Trace(string message)
        {
            _tracingService?.Trace($"[PluginContextSerializer] {message}");
        }
    }
}
