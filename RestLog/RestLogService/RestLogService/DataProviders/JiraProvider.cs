using Atlassian.Jira;
using RestLogService.Config;
using RestLogService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace RestLogService.DataProviders
{
    public class JiraInfoProvider
    {


        internal static List<JiraInfo> SearchJiraByLabel(string label)
        {
            List<JiraInfo> jiras = new List<JiraInfo>();

            var jiraHandler = Jira.CreateRestClient(ConfigProvider.JiraUrl, ConfigProvider.JiraUsername, ConfigProvider.JiraPsw);
            var issues = from i in jiraHandler.Issues.Queryable
                         where i.Project == ConfigProvider.JiraProject && i.Labels == label
                         orderby i.Updated descending
                         select i;

            foreach (Issue issue in issues) {
                jiras.Add(new JiraInfo(issue));
            }
            return jiras;
        }

        internal static List<JiraInfo> SearchJiraByKeys(string project, string label, List<string> keys)
        {
            List<JiraInfo> jiras = new List<JiraInfo>();
            var jiraHandler = Jira.CreateRestClient(ConfigProvider.JiraUrl, ConfigProvider.JiraUsername, ConfigProvider.JiraPsw);

            string jiraList = string.Empty;

            foreach(string key in keys)
            {
                if (jiraList != string.Empty)
                {
                    jiraList += ",";
                }
                jiraList += key;
            }
            string jql = string.Format("project={0} AND labels={1} and id in ({2}) ORDER BY updated DESC", project, label, jiraList);

            var issues = jiraHandler.Issues.GetIssuesFromJqlAsync(jql).Result;
            foreach (Issue issue in issues)
            {
                jiras.Add(new JiraInfo(issue));
            }
            return jiras;
        }   

    }
}
 