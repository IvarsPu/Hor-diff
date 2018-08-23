using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RestLogService.Config;
using RestLogService.DataProviders;
using RestLogService.Models;

namespace RestLogService.Controllers
{
    [RoutePrefix("api/versions")]
    public class VersionsController : ApiController
    {
        /*
        private static List<RestVersion> restVersions { get; set; }
        private static List<JiraInfo> testJiraInfos { get; set; }

       static VersionsController()
               {
                   VersionsController.CreateTestData();
               }
               private static void CreateTestData()
               {
                   VersionsController.restVersions = new List<RestVersion>() {
                     //  new RestVersion("Visās", ""),
                     //  new RestVersion("trunk", ""),
                       new RestVersion("520", ""),
                       new RestVersion("515", ""),
                       new RestVersion("510", ""),
                       new RestVersion("500", ""),
                       new RestVersion("495", ""),
                   };

                   string now = DateTime.Now.ToString("g");
                   VersionsController.testJiraInfos = new List<JiraInfo>() {
                       new JiraInfo("HOR-138427", now, "Security Self Asessement", "Closed", "High", "Bug", "TadmLaikaDok",
                           "Ceļš: Dokumenti -> Algas -> Algas aprēķins<br />" +
                           "Problēma: Ja ir aprēķināta prombūtne par vairākiem mēnešiem uz priekšu un tika pielietoti atvieglojumi, tad nākamajā mēnesī, kad nostrādāts 0 stundas, tika atskaitīta summa par atvieglojumiem.<br />" +
                           "Risinājums: Problēma novērsta. Ja ir bijuši nepareizi atskaitīti atvieglojumi, tad veicot pārrēķinu, tie tiek pieskaitīti atpakaļ.<br />" +
                           "REST ietekme: TadmLaikaDok objektam pievienots papildus obligātais lauks IR_PIEMCTRL<br />"),
                       new JiraInfo("HOR-138540", now, "Security Self Assesment: Web sistēmas un mobilās programmas", "Closed", "High", "Bug", "TdmPDok, TdmProject, TdmSatl, TdmBLValSv", ""),
                       new JiraInfo("HOR-142496", now, "Horizon SA20 - Review dynamic SQL usage", "Closed", "High", "Bug", "", ""),
                       new JiraInfo("HOR-142539", now, "Horizon SA60 - Consider to inform users about the change of the password via the email", "Closed", "High", "Bug", "", ""),
                       new JiraInfo("HOR-142536", now, "Horizon SA63 - Controls to detect compromised account", "Closed", "High", "Bug", "", ""),
                       new JiraInfo("HOR-142500", now, "Horizon - Consider to run Horizon upgrade and application logic under different DB user roles", "Closed", "High", "Bug", "", ""),
                       new JiraInfo("HOR-142492", now, "Horizon - Replace client-side stored DB credentials with login service", "Closed", "High", "Bug", "TdmgSakAtlikBL", ""),
                       new JiraInfo("HOR-142491", now, "Horizon - Encrypt bug report files", "Closed", "High", "Bug", "", ""),
                       new JiraInfo("HOR-142488", now, "Horizon SA27 - set secure cookie attributes", "Closed", "High", "Bug", "TdmESPProjPazimSL", ""),
                       new JiraInfo("HOR-142487", now, "Horizon SA59 - fix HTTP security headers", "Closed", "High", "Bug", "", ""),
                       new JiraInfo("HOR-142485", now, "Horizon REST SA59 - Close public access to /rest/global/files endpoint", "Closed", "High", "Bug", "TdmVaDSektSL, TdmFinans", ""),
                       new JiraInfo("HOR-142484", now, "Horizon REST SA59 - service does not supports xml and json schema validation", "Closed", "High", "Bug", "", "")
                   };
               }
               */
        [Route("")]
        [HttpGet]
        public IList<RestVersionBase> GetRestVerions()
        {
            List<RestVersion> versions = ConfigProvider.versions;

            List<RestVersionBase> plainVersions = new List<RestVersionBase>();
            versions.ForEach(item => plainVersions.Add(item));

            return plainVersions;
        }

        [Route("{version}/filterJiras")]
        [HttpGet]
        public IList<JiraInfo> GetLoggedJiraInfos(string version)
        {
            RestVersion versionInfo = ConfigProvider.getVersionInfo(version);
            if (versionInfo == null)
            {
                // Input from hacker
                return null;
            }

            List<JiraInfo> result = JiraInfoProvider.SearchJiraByLabel(ConfigProvider.RestAlertLabel);

            if (version != "Visas")
            {
                result = SvnProvider.FilterJiraByVersionCommits(result, version);
            }

            return result;
        }
    }
}
