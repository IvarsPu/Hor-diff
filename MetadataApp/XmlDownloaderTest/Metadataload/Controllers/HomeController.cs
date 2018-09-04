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
        public static SortedDictionary<int, Process> processes = new SortedDictionary<int, Process>();

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

            Process process = new Process(processId, DateTime.Now);
            processes.Add(processId, process);
            
            Task.Run(() => new Program().DoTheJob());

            return processId;
        }

        ////will be replaced with new method
        //public void DoProcessing(Process process)
        //{
        //    for(int i = 0; i<=100;i++)
        //    {
        //        if (process.Token.IsCancellationRequested)
        //        {
        //            //work stopped
        //            process.Status = "Stopped";
        //            process.Token.ThrowIfCancellationRequested();
        //        }
        //        //working
        //        process.Progress = i;

        //        Thread.Sleep(600);//does something for 0.5 sec
        //    }

        //    //work ends
        //    process.EndTime = DateTime.Now;
        //    process.Status = "Done";
        //    process.Done = true;
        //}
        
        //http://localhost:49936/Home/GetProcessStatus?processId=1
        [Route("GetProcessStatus")]
        [HttpGet]
        public Process GetProcessStatus(int processId)
        {
            return processes.TryGetValue(processId, out Process value) ? value : null;
        }

        //http://localhost:49936/Home/StopProcess?processId=1
        [Route("StopProcess")]
        [HttpGet]
        public KeyValuePair<bool,string> StopProcess(int processId)
        {
            try
            {
                processes[processId].TokenSource.Cancel();
                return new KeyValuePair<bool, string>(true, "");
            }
            catch
            {
                return new KeyValuePair<bool, string>(false, "Element doesnt exist");
            }
            //if (processes.Remove(processId)) do we need to clean dictionary?
        }

        //http://localhost:49936/Home/GetProcessList?noOfProcesses=100
        [Route("GetProcessList")]
        [HttpGet]
        public List<Process> GetProcessList(int noOfProcesses = 10)
        {
            return processes.Reverse().ToDictionary(x => x.Key, x => x.Value).Values.Take(noOfProcesses).ToList();
        }
    }
}