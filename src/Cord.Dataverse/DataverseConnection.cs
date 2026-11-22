using Azure.Core;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Cord.Dataverse;

public static class DataverseConnection
{
    /// <summary>
    /// A <see cref="ServiceClient"/> that authenticates with an Entra ID token credential, e.g.
    /// <c>DefaultAzureCredential</c>: the Function App's managed identity in Azure, your
    /// <c>az login</c> / Visual Studio sign-in locally. No client secret to store or rotate.
    /// The identity must be an application user in the environment, with a security role that can write <c>cord_waybill</c>.
    /// </summary>
    public static ServiceClient Create(Uri environmentUrl, TokenCredential credential) =>
        new(environmentUrl, async instanceUri =>
        {
            var scope = new Uri(instanceUri).GetLeftPart(UriPartial.Authority) + "/.default";
            var token = await credential.GetTokenAsync(new TokenRequestContext([scope]), CancellationToken.None);
            return token.Token;
        });
}
