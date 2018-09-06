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
        public static SortedDictionary<int, Process> Processes { get; set; } = new SortedDictionary<int, Process>();

        //http://localhost:49936/Metadata/StartMetadataLoad?versionId=1
        [Route("StartMetadataLoad")]
        [HttpGet]
        public int StartMetadataLoad(int versionId)
        {
            int processId = 1;
            if (Processes.Count > 0)
            {
                processId = Processes.Last().Key + 1;
            }

            Process process = new Process(processId, DateTime.Now);
            Processes.Add(processId, process);

            //System.Threading.Tasks.Task.Run(() => new Program().DoTheJob(processId));

            return processId;
        }

        //http://localhost:49936/Metadata/GetProcessStatus?processId=1
        [Route("GetProcessStatus")]
        [HttpGet]
        public Process GetProcessStatus(int processId)
        {
            return Processes.TryGetValue(processId, out Process value) ? value : null;
        }

        //http://localhost:49936/Metadata/StopProcess?processId=2
        [Route("StopProcess")]
        [HttpGet]
        public KeyValuePair<bool, string> StopProcess(int processId)
        {
            try
            {
                Processes[processId].TokenSource.Cancel();
                return new KeyValuePair<bool, string>(true, "");
            }
            catch
            {
                return new KeyValuePair<bool, string>(false, "Element doesnt exist");
            }
            //if (processes.Remove(processId)) do we need to clean dictionary?
        }

        //http://localhost:49936/Metadata/GetProcessList?noOfProcesses=100
        [Route("GetProcessList")]
        [HttpGet]
        public List<Process> GetProcessList(int noOfProcesses = 10)
        {
            return Processes.Reverse().ToDictionary(x => x.Key, x => x.Value).Values.Take(noOfProcesses).ToList();
        }
    }
}
