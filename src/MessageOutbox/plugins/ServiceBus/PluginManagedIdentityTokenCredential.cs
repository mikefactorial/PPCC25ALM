using Azure.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBus
{
    /// <summary>
    /// Custom TokenCredential implementation that uses a managed identity access token from Dataverse
    /// </summary>
    public class PluginManagedIdentityTokenCredential : TokenCredential
    {
        private readonly string _accessToken;
        private readonly DateTimeOffset _expiresOn;

        /// <summary>
        /// Initializes a new instance of the ManagedIdentityTokenCredential class
        /// </summary>
        /// <param name="accessToken">The access token string obtained from IManagedIdentityService</param>
        /// <param name="expiresOn">Optional expiration time. Defaults to 1 hour from now if not specified</param>
        public PluginManagedIdentityTokenCredential(string accessToken, DateTimeOffset? expiresOn = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            _accessToken = accessToken;
            _expiresOn = expiresOn ?? DateTimeOffset.UtcNow.AddHours(1);
        }

        /// <summary>
        /// Gets an AccessToken for the specified request context
        /// </summary>
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new AccessToken(_accessToken, _expiresOn);
        }

        /// <summary>
        /// Gets an AccessToken for the specified request context asynchronously
        /// </summary>
        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(new AccessToken(_accessToken, _expiresOn));
        }
    }
}
