using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Xml;
using DiffRest.Models;

namespace DiffRest.Controllers
{
    [RoutePrefix("Metadata")]
    public class MetadataController : ApiController
    {
        [HttpGet]
        [Route("GetProcessList")]
        public List<Process> GetProcessList(int noOfProcesses = 10)
        {
            List<Process> processList = new List<Process>();
            foreach (KeyValuePair<int, Process> pair in ProcessController.Processes.OrderByDescending(x => x.Key).Take(noOfProcesses))
            {
                processList.Add(pair.Value);
            }
            return processList;
        }

        [HttpGet]
        [Route("GetProfiles")]
        public List<Profile> GetProfiles()
        {
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(ProfileController.path))
            {
                doc.Load(ProfileController.path);
                List<Profile> profiles = new List<Profile>();
                foreach (XmlNode node in doc.SelectNodes("//Profile"))
                {
                    try
                    {
                        Profile profile = new Profile();
                        profile.Id = Int32.Parse(node.Attributes["ID"].Value);
                        profile.Name = node.Attributes["Name"].Value;
                        profile.Url = node.Attributes["Url"].Value;
                        profile.Username = node.Attributes["Username"].Value;
                        profile.Password = node.Attributes["Password"].Value;
                        profiles.Add(profile);
                    }
                    catch { }
                }
                return profiles;
            }
            return null;
        }
    }
}