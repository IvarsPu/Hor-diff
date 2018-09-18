using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Metadataload.Models;

namespace Metadataload.Controllers
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
        public int StartMetadataLoad(string version)
        {
            try
            {
                int processId = 1;
                int id = Int32.Parse(Session[ProfileController.profileId].ToString());
                if (Processes.Count > 0)
                {
                    foreach (Process pr in Processes.Values)
                    {
                        if (pr.ServerId == id && !pr.Done)
                        {
                            return 0;
                        }
                    }
                    processId = Processes.Last().Key + 1;
                }

                Process process = new Process(processId, id, DateTime.Now);
                Processes.Add(processId, process);

                //System.Threading.Tasks.Task.Run(() => new Program().DoTheJob(processId));

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
                if (Processes[processId].ServerId.ToString().Equals(Session[ProfileController.profileId]))
                {
                    Processes[processId].TokenSource.Cancel();
                    return "";
                }
                return "No access";
            }
            catch
            {
                return "Element doesnt exist";
            }
            //if (processes.Remove(processId)) do we need to clean dictionary?
        }
    }
}