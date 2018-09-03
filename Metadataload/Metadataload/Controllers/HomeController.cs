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

            Process process = new Process(DateTime.Now, new DateTime(), false, 0);
            processes.Add(processId, process);

            Work(processId);

            return processId;
        }

        private void Work(int processId)
        {
            var progress = new Progress<Process>(process =>
            {
                processes[processId] = process;
            });

            Task.Run(() => DoProcessing(progress, processes[processId]));
        }

        public void DoProcessing(IProgress<Process> progress, Process process)
        {
            for (int i = 0; i != 100; ++i)
            {
                Thread.Sleep(1000);
                //work in progress
                process.Progress = i;
                progress.Report(process);
            }

            //work ends
            process.EndTime = DateTime.Now;
            process.Done = true;
            progress.Report(process);
        }





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
            return new KeyValuePair<bool, string>(processes.Remove(processId), "test");
        }

        [Route("GetProcessList")]
        [HttpGet]
        public List<Process> GetProcessList(int processNumber)
        {
            return null;
        }
    }
}