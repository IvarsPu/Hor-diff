using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Metadataload.Models;

namespace Metadataload.Controllers
{
    [RoutePrefix("Home")]
    public class HomeController : ApiController
    {
        private static SortedDictionary<int, Process> processes = new SortedDictionary<int, Process>();

        //http://localhost:49936/Home/StartMetadataLoad?versionId=1
        [Route("StartMetadataLoad")]
        [HttpGet]
        public int StartMetadataLoad(int versionId)
        {
            int processId = 1;
            if (processes.Count > 0)
            { 
                processId = processes.Last().Key + 1;
            }

            Process process = new Process(processId, DateTime.Now, new DateTime(), false, 0);
            processes.Add(processId, process);

            Task.Run(() => DoProcessing(process));

            return processId;
        }

        public void DoProcessing(Process process)
        {
            int i = 0;
            //while process isnt canceled
            while (!process.Token.IsCancellationRequested)
            {
                //work in progress
                process.Progress = i;
                i++;
                Thread.Sleep(100);
            }

            //work ends
            process.EndTime = DateTime.Now;
            process.Done = true;
        }
        
        //http://localhost:49936/Home/GetProcessStatus?processId=1
        [Route("GetProcessStatus")]
        [HttpGet]
        public Process GetProcessStatus(int processId)
        {
            return processes.TryGetValue(processId, out Process value) ? value : null;
        }

        //http://localhost:49936/Home/StopProcess?processId=2
        [Route("StopProcess")]
        [HttpGet]
        public KeyValuePair<bool,string> StopProcess(int processId)
        {
            try
            {
                processes[processId].TokenSource.Cancel();
                return new KeyValuePair<bool, string>(true, "Sucessfully stopped");
            }
            catch
            {
                return new KeyValuePair<bool, string>(false, "Element doesnt exist");
            }
            //if (processes.Remove(processId))
        }

        //http://localhost:49936/Home/GetProcessList
        [Route("GetProcessList")]
        [HttpGet]
        public List<Process> GetProcessList(int processNumber = 10)
        {
            return processes.Reverse().ToDictionary(x => x.Key, x => x.Value).Values.Take(processNumber).ToList();
        }
    }
}