using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("Process")]
    public class ProcessController : Controller
    {
        public static SortedDictionary<int, Process> Processes { get; set; } = new SortedDictionary<int, Process>();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Info(int processID)
        {
            ViewBag.Process = Processes.TryGetValue(processID, out Process value) ? value : null;
            return View();
        }
        
        [Route("StartMetadataLoad")]
        public void StartMetadataLoad(int metadataServiceId)
        {
            if (MetadataServiceController.GetMetadataService(metadataServiceId) != null)
            {

                int processId = 1;
                if (Processes.Count > 0)
                {
                    foreach (Process processValue in Processes.Values)
                    {
                        if (processValue.MetadataServiceId == metadataServiceId && !processValue.Done)
                        {
                            throw new Exception();
                        }
                    }
                    processId = Processes.Last().Key + 1;
                }

                Process process = new Process(processId, metadataServiceId, DateTime.Now);
                Processes.Add(processId, process);

                System.Threading.Tasks.Task.Run(() => new Program().DoTheJob(processId));
            }
            else
            {
                throw new Exception();
            }
        }
        
        [Route("StopProcess")]
        public void StopProcess(int processId)
        {
            Processes[processId].TokenSource.Cancel();
            //if (processes.Remove(processId)) do we need to clean dictionary?
        }
    }
}