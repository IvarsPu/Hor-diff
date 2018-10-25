using System.Collections.Generic;
using System.Web.Http;
using BusinessLogic;
using Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("List")]
    public class ListController : ApiController
    {
        [Route("GetRESTMetadataList")]
        public List<Process> GetRESTMetadataList(int noOfProcesses = 10)
        {
            return new DownloadMetadata().GetRESTMetadataList(noOfProcesses);
        }

        [Route("GetConnections")]
        public List<RestConnection> GetConnections()
        {
            return new Connection().GetConnections();
        }

        //http://localhost:5001/List/CompareFiles?oldRelease=515/13&newRelease=515/21
        [Route("CompareFiles")]
        [HttpGet]
        public List<Service> CompareFiles(string oldRelease, string newRelease, bool noChange = false, bool added = true, bool ignoreNamespaceChanges = false)
        {
            return new CompareFiles().Compare(oldRelease, newRelease, noChange, added, ignoreNamespaceChanges);
        }
    }
}