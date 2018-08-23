using RestLogService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace RestLogService.Config
{
    public class ConfigProvider
    {
        private static string ParamJiraProject = System.Configuration.ConfigurationManager.AppSettings["jiraProject"];
        private static string ParamJiraUrl = System.Configuration.ConfigurationManager.AppSettings["jiraUrl"];
        private static string ParamJiraSvnUsername = System.Configuration.ConfigurationManager.AppSettings["jiraSvnUser"];
        private static string ParamJiraSvnPsw = System.Configuration.ConfigurationManager.AppSettings["jiraSvnPsw"];
        private static string ParamRestAlertLabel = System.Configuration.ConfigurationManager.AppSettings["jiraRestAlertLabel"];
        private static string ParamSvnJiraRegex = System.Configuration.ConfigurationManager.AppSettings["svnJiraRegex"];
        private static string ParamSvnReleaseRegex = System.Configuration.ConfigurationManager.AppSettings["svnReleaseRegex"];
        private static List<RestVersion> paramVersions = getConfigVersions();
  
        public static string JiraProject
        {
            get
            {
                return ParamJiraProject;
            }
        }

        public static string JiraUrl {
            get
            {
                return ParamJiraUrl;
            }
        }

        public static string RestAlertLabel
        {
            get
            {
                return ParamRestAlertLabel;
            }
        }
        
        public static string SvnUsername
        {
            get
            {
                return ParamJiraSvnUsername;
            }
        }

        public static string JiraUsername
        {
            get
            {
                return ParamJiraSvnUsername;
            }
        }

        public static string SvnPsw
        {
            get
            {
                return ParamJiraSvnPsw;
            }
        }

        public static string JiraPsw
        {
            get
            {
                return ParamJiraSvnPsw;
            }
        }


        public static string SvnJiraRegex
        {
            get
            {
                return ParamSvnJiraRegex;
            }
        }
        
        public static string SvnReleaseRegex
        {
            get
            {
                return ParamSvnReleaseRegex;
            }
        }

        public static List<RestVersion> versions
        {
            get
            {
                return paramVersions;
            }
        }

        public static RestVersion getVersionInfo(string version)
        {
            RestVersion result = null;

            foreach (RestVersion ver in paramVersions)
            {
                if (ver.Name == version)
                {
                    result = ver;
                    break;
                }
            }

            return result;
        }

        private static List<RestVersion> getConfigVersions()
        {
            List<RestVersion> restVersions = new List<RestVersion>();

            var config = ConfigurationManager.GetSection("restVersions")
                as VersionConfigSection;

            foreach (VersionConfigInstanceElement e in config.Instances)
            {
                restVersions.Add(new RestVersion(e.Name, e.SvnUrl, e.FirstRevision));
            }

            return restVersions;
        }
    }
}