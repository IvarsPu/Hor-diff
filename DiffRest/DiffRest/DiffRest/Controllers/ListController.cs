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
        
        [Route("GetMetadataServices")]
        public List<MetadataService> GetMetadataServices()
        {
            return new ServerConn().GetMetadataServices();
        }

        [Route("CompareFiles")]
        public List<Service> CompareFiles(string oldRelease, string newRelease, bool noChange = false, bool added = true, bool ignoreNamespaceChanges = false)
        {
            return new CompareFiles().Compare(oldRelease, newRelease, noChange, added, ignoreNamespaceChanges);
        }
    }
}