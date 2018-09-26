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
        
        [HttpGet]
        [Route("StartMetadataLoad")]
        public int StartMetadataLoad(int version)
        {
            try
            {
                int processId = 1;
                if (Processes.Count > 0)
                {
                    foreach (Process pr in Processes.Values)
                    {
                        if (pr.ServerId == version && !pr.Done)
                        {
                            return 0;
                        }
                    }
                    processId = Processes.Last().Key + 1;
                }

                Process process = new Process(processId, version, DateTime.Now);
                Processes.Add(processId, process);

                //System.Threading.Tasks.Task.Run(() => new Program().DoTheJob(processId), process.Token);

                return processId;
            }
            catch
            {
                return 0;
            }
        }
        
        [HttpGet]
        [Route("StopProcess")]
        public string StopProcess(int processId)
        {
            try
            {
                Processes[processId].TokenSource.Cancel();
                return "";
            }
            catch
            {
                return "Element doesnt exist";
            }
            //if (processes.Remove(processId)) do we need to clean dictionary?
        }
    }
}