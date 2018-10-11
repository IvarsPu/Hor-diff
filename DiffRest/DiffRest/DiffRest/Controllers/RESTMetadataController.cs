﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("RESTMetadata")]
    public class RESTMetadataController : Controller
    {
        public static SortedDictionary<int, Process> Processes { get; set; } = new SortedDictionary<int, Process>();

        public ActionResult Index()
        {
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

        public bool AlreadyExists(int processId)
        {
            MetadataService profile = MetadataServiceController.GetMetadataService(Processes[processId].MetadataServiceId);
            Models.AppContext appContext = new Models.AppContext(profile.Url, profile.Username, profile.Password, HomeController.MetadataRootFolder);
            Logger.LogPath = appContext.RootLocalPath;
            XmlMetadata xmlMetadata = new XmlMetadata(appContext);
            WebResourceLoader webResourceLoader = new WebResourceLoader(appContext, xmlMetadata, processId);
            xmlMetadata.InitServiceMetadata(webResourceLoader);

            XmlDocument doc = new XmlDocument();
            doc.Load(HomeController.MetadataRootFolder + "Versions.xml");
            XmlNode node = doc.SelectSingleNode("//version[@name='" + appContext.Version + "']/release[@name='" + appContext.Release + "']");
            if (node != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public ActionResult Info(int processId)
        {
            Process metadata = Processes.TryGetValue(processId, out Process value) ? value : null;
            return View(metadata);
        }
        
        [Route("StopProcess")]
        public void StopProcess(int processId)
        {
            Processes[processId].TokenSource.Cancel();
        }
    }
}