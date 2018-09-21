using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("Metadata")]
    public class MetadataController : ApiController
    {
        [HttpGet]
        [Route("GetProcessList")]
        public List<Process> GetProcessList(int noOfProcesses = 10)
        {
            List<Process> processList = new List<Process>();
            foreach (KeyValuePair<int, Process> pair in ProcessController.Processes.OrderByDescending(x => x.Key).Take(noOfProcesses))
            {
                processList.Add(pair.Value);
            }
            return processList;
        }
    }
}