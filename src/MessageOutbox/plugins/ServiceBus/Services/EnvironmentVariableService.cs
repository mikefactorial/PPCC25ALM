using Microsoft.Xrm.Sdk;
using System;

namespace ServiceBus.Services
{
    /// <summary>
    /// Service for retrieving environment variables from Dataverse
    /// </summary>
    public class EnvironmentVariableService
    {
        private readonly IOrganizationService _organizationService;
        private readonly ITracingService _tracingService;

        /// <summary>
        /// Initializes a new instance of the EnvironmentVariableService class
        /// </summary>
        /// <param name="organizationService">The organization service to use for retrieving environment variables</param>
        /// <param name="tracingService">Optional tracing service for logging</param>
        public EnvironmentVariableService(
            IOrganizationService organizationService,
            ITracingService tracingService = null)
        {
            _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _tracingService = tracingService;
        }

        /// <summary>
        /// Retrieves the value of an environment variable by its schema name
        /// </summary>
        /// <param name="schemaName">The schema name of the environment variable (e.g., "publisher_ServiceBusNamespace")</param>
        /// <returns>The environment variable value as a string</returns>
        public string GetValue(string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                throw new ArgumentNullException(nameof(schemaName));
            }

            try
            {
                Trace($"Retrieving environment variable: {schemaName}");

                OrganizationRequest request = new OrganizationRequest("RetrieveEnvironmentVariableValue");
                request.Parameters["DefinitionSchemaName"] = schemaName;

                var response = _organizationService.Execute(request);
                var value = response.Results["Value"]?.ToString();

                Trace($"Environment variable '{schemaName}' retrieved successfully");
                return value;
            }
            catch (Exception ex)
            {
                Trace($"Error retrieving environment variable '{schemaName}': {ex.Message}");
                throw new InvalidPluginExecutionException($"Failed to retrieve environment variable '{schemaName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes a trace message if tracing service is available
        /// </summary>
        private void Trace(string message)
        {
            _tracingService?.Trace($"[EnvironmentVariableService] {message}");
        }
    }
}
