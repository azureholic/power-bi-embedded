using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddUserSecrets<Program>()
            .Build();

var servicePrincipalClientId = config["ClientId"];
var servicePrincipalClientSecret = config["ClientSecret"];
var azureAdTenantId = config["TenantId"];
var azureAdContext = $"https://login.microsoftonline.com/{azureAdTenantId}";


var app = ConfidentialClientApplicationBuilder.Create(servicePrincipalClientId)
    .WithClientSecret(servicePrincipalClientSecret)
    .WithAuthority(new Uri(azureAdContext))
    .Build();

string[] scopes = new string[] { "api://7a4f1274-b772-48b0-8e3b-62bd865bd30b/.default" };
var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

Console.WriteLine(authResult.AccessToken);

using (var client = new HttpClient())
{

    client.DefaultRequestHeaders.Accept.Clear();
    var auth = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
    client.DefaultRequestHeaders.Authorization = auth;
  
    HttpResponseMessage response = client.GetAsync("https://localhost:7025/tenants").Result;

    Console.WriteLine((int)response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();

    Console.WriteLine(content);
   


}

