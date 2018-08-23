using RestLogService.Config;
using RestLogService.Models;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace RestLogService.DataProviders
{
    public class SvnProvider
    {

        private static List<String> GetSvnJiras(RestVersion versionInfo)
        {
            Collection<SvnLogEventArgs> logitems;
            List<String> result = new List<String>();
            string jiraProject = ConfigProvider.JiraProject;

            using (SvnClient client = new SvnClient())
            {
                client.Authentication.DefaultCredentials = new NetworkCredential(ConfigProvider.SvnUsername, ConfigProvider.SvnPsw);
                client.LoadConfiguration(Path.Combine(Path.GetTempPath(), "Svn"), true);

                Uri svnrepo = new Uri(versionInfo.SvnUrl); // "svn://servername/reponame/");
                SvnInfoEventArgs info;
                client.GetInfo(svnrepo, out info);
                long lastRev = info.Revision;

                long endRev = lastRev - 999;
                if (versionInfo.FirstRevision != null && versionInfo.FirstRevision != string.Empty)
                {
                    endRev = Convert.ToInt64(versionInfo.FirstRevision);
                }
                SvnLogArgs args = new SvnLogArgs { Start = lastRev, End = endRev, };
                args.RetrieveAllProperties = true;
                client.GetLog(svnrepo, args, out logitems);
            }

            Regex jiraRegex = new Regex(ConfigProvider.SvnJiraRegex);
            Regex releaseRegex = new Regex(ConfigProvider.SvnReleaseRegex);
            Match match;
            foreach (var logentry in logitems)
            {
                string message = logentry.LogMessage.ToUpper();
                string jira = string.Empty;
                match = jiraRegex.Match(message);
                if (match.Success)
                {
                    jira = match.Value;
                }
                string author = logentry.Author;
                message = logentry.LogMessage;

                if (jira == string.Empty)
                {
                    match = releaseRegex.Match(message);
                    if (match.Success)
                    {
                        //Add release info to list
                        result.Add(message);
                    }
                }
                else if (!result.Contains(jira))
                {
                    result.Add(jira);
                }
              
            }
            return result;
        }

        public static List<JiraInfo> FilterJiraByVersionCommits(List<JiraInfo> labeledJiras, string version)
        {
            List<JiraInfo> result = new List<JiraInfo>();

            RestVersion versionInfo = ConfigProvider.getVersionInfo(version);
            String jiraProject = ConfigProvider.JiraProject;
            List<String> svnLog = GetSvnJiras(versionInfo);

            Dictionary<String, JiraInfo> jiraLookup = new Dictionary<String, JiraInfo>();
            foreach (JiraInfo jira in labeledJiras)
            {
                jiraLookup.Add(jira.Key.ToUpper(), jira);
            }

            JiraInfo jiraInfo;
            foreach (String line in svnLog)
            {
                if (line.Contains(jiraProject))                    
                {
                    // we have jira
                    if (jiraLookup.TryGetValue(line.ToUpper(), out jiraInfo))
                    {
                        result.Add(jiraInfo);
                    }                    
                }
                else
                {
                    // Release label
                    result.Add(new JiraInfo("", line));
                }
            }

            return result;
        }
    }
}