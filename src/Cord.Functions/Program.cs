using Azure.Identity;
using Cord.Core;
using Cord.Dataverse;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;

var builder = FunctionsApplication.CreateBuilder(args);

// App setting "Dataverse__Url", e.g. https://contoso.crm4.dynamics.com
var dataverseUrl = builder.Configuration["Dataverse:Url"]
    ?? throw new InvalidOperationException("The Dataverse:Url setting (Dataverse__Url) is missing.");

builder.Services
    .AddSingleton<IOrganizationServiceAsync2>(_ => DataverseConnection.Create(new Uri(dataverseUrl), new DefaultAzureCredential()))
    .AddSingleton<IWaybillStore, DataverseWaybillStore>()
    .AddSingleton<WaybillImport>();

builder.Build().Run();
