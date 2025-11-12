using Microsoft.Xrm.Sdk;
using System;

namespace ServiceBus.Services
{
    /// <summary>
    /// Configuration for Service Bus connection
    /// </summary>
    public class ServiceBusConfiguration
    {
        /// <summary>
        /// Gets or sets the Service Bus namespace (e.g., myservicebus.servicebus.windows.net)
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the queue name
        /// </summary>
        public string QueueName { get; set; }

    }
}
