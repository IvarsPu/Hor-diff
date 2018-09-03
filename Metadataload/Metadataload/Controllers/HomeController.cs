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
            
            var progress = new Progress<Process>(newProcess =>
            {
                process = newProcess;
            });

            Task.Run(() => DoProcessing(progress, process));

            return processId;
        }

        public void DoProcessing(IProgress<Process> progress, Process process)
        {
            int i = 0;
            //while process isnt canceled
            while (!process.Token.IsCancellationRequested)
            {
                Thread.Sleep(100);
                //work in progress
                process.Progress = i;
                progress.Report(process);
                i++;
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
            processes[processId].TokenSource.Cancel();
            //if (processes.Remove(processId))
            //{
                return new KeyValuePair<bool, string>(true, "Sucessfully removed");
            //}
            //else
            //{
            //    return new KeyValuePair<bool, string>(false, "Element doesnt exist");
            //}
        }

        //http://localhost:49936/Home/GetProcessList?processNumber=1
        [Route("GetProcessList")]
        [HttpGet]
        public List<Process> GetProcessList(int processNumber)
        {
            return processes.Reverse().ToDictionary(x => x.Key, x => x.Value).Values.Take(processNumber).ToList();
        }
    }
}