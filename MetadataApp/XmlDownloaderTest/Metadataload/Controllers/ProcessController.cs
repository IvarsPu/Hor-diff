using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
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
            ViewBag.Process = GetProcessStatus(processID);
            return View();
        }

        [Route("StartMetadataLoad")]
        [HttpGet]
        public int StartMetadataLoad(string version)
        {
            int processId = 1;
            int id = Int32.Parse(Session["userId"].ToString());
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

        [Route("GetProcessStatus")]
        [HttpGet]
        public Process GetProcessStatus(int processId)
        {
            return Processes.TryGetValue(processId, out Process value) ? value : null;
        }

        [Route("StopProcess")]
        [HttpGet]
        public string StopProcess(int processId)
        {
            int id = Int32.Parse(Session["userId"].ToString());
            try
            {
                if (Processes[processId].ServerId == id)
                {
                    Processes[processId].TokenSource.Cancel();
                    return "";
                }
                return "no access";
            }
            catch
            {
                return "Element doesnt exist";
            }
            //if (processes.Remove(processId)) do we need to clean dictionary?
        }
    }
}