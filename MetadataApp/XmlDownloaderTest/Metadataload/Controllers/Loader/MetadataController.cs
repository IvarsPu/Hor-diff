using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Metadataload.Models;

namespace Metadataload.Controllers
{
    [RoutePrefix("Metadata")]
    public class MetadataController : ApiController
    {
        [Route("GetProcessList")]
        [HttpGet]
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