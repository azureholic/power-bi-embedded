using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Azure.Core;
using Microsoft.PowerBI.Api;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System.Runtime.InteropServices;
using Microsoft.PowerBI.Api.Models;

namespace Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly IConfiguration _config;
        const string powerBiApiUrl = "https://api.powerbi.com";

        public TenantsController(IConfiguration config) {

            _config = config;
        }

       
        
        [HttpGet]
        public ActionResult Get() {

            var t = new Tenant();
            t.PbiClientId = _config["Tenant:ClientId"];
            t.PbiClientSecret = _config["Tenant:ClientSecret"];
            t.PbiWorkspace = _config["Tenant:WorkspaceId"];

            var pbiClientApp = ConfidentialClientApplicationBuilder
                .Create(_config["Tenant:ClientId"])
                .WithClientSecret(_config["Tenant:ClientSecret"])
                .WithAuthority(_config["Tenant:AuthorityUrl"])
                .Build();

            // Make a client call if Access token is not available in cache
            var authenticationResult = pbiClientApp.AcquireTokenForClient(
                    new string[] { _config["Tenant:ScopeBase"] }
                    ).ExecuteAsync().Result;

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            var pbiClient = new PowerBIClient(new Uri(powerBiApiUrl), tokenCredentials);

            var workspace = new Guid(_config["Tenant:WorkspaceId"]);

            var reports = pbiClient.Reports.GetReports(workspace);



            List<Guid> reportIds = new List<Guid>();
            foreach (var report in reports.Value)
            {
                reportIds.Add(report.Id);
            }

            var embedParams = GetEmbedParams(pbiClient, workspace, reportIds);
            
            return Ok(embedParams);
            
        }

        /// <summary>
        /// Get embed params for multiple reports for a single workspace
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for multiple reports</returns>
        /// <remarks>This function is not supported for RDL Report</remakrs>
        private EmbedParams GetEmbedParams(PowerBIClient pbiClient, Guid workspaceId, IList<Guid> reportIds, [Optional] IList<Guid> additionalDatasetIds)
        {
            // Create mapping for reports and Embed URLs
            var embedReports = new List<EmbedReport>();

            // Create list of datasets
            var datasetIds = new List<Guid>();

            // Get datasets and Embed URLs for all the reports
            foreach (var reportId in reportIds)
            {
                // Get report info
                var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

                datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

                // Add report data for embedding
                embedReports.Add(new EmbedReport { ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl });
            }

            // Append to existing list of datasets to achieve dynamic binding later
            if (additionalDatasetIds != null)
            {
                datasetIds.AddRange(additionalDatasetIds);
            }

            // Get Embed token multiple resources
            var embedToken = GetEmbedToken(pbiClient, reportIds, datasetIds, workspaceId);

            // Capture embed params
            var embedParams = new EmbedParams
            {
                EmbedReport = embedReports,
                Type = "Report",
                EmbedToken = embedToken
            };

            return embedParams;
        }

        private EmbedToken GetEmbedToken(PowerBIClient pbiClient, IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
        {
            

            // Convert report Ids to required types
            var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

            // Convert dataset Ids to required types
            var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

            // Create a request for getting Embed token 
            // This method works only with new Power BI V2 workspace experience
            var tokenRequest = new GenerateTokenRequestV2(
                datasets: datasets,
                reports: reports,
                targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
            );

            // Generate Embed token
            var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

            return embedToken;
        }

    }
}
