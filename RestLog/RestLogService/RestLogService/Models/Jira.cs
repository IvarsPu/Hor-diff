using Atlassian.Jira;
using RestLogService.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;

namespace RestLogService.Models
{
    [Serializable]
    [DataContract(Name = "jira")]
    public class JiraInfo
    {
        public JiraInfo(string key, string summary)
        {
            Key = key;
            Summary = summary;
        }

        public JiraInfo(Issue issue)
        {
            Key = issue.Key.Value;
            LastFixDate = issue.Updated.Value.ToString("g"); 
            Summary = issue.Summary;
            Status = issue.Status.Name;
            Priority = issue.Priority.Name;
            IssueType = issue.Type.Name;

            if ( issue["Release Notes."]!= null)
            {
                initReleaseNotesAndBlList(issue["Release Notes."].Value);
            }
            Url = getJiraUrl(Key);
        }
        

        [DataMember(Name = "key")]
        public string Key { get; set; } = String.Empty;


        [DataMember(Name = "lastFixDate")]
        public string LastFixDate { get; set; } = String.Empty;

        [DataMember(Name = "summary")]
        public string Summary { get; set; } = String.Empty;

        [DataMember(Name = "status")]
        public string Status { get; set; } = String.Empty;

        [DataMember(Name = "priority")]
        public string Priority { get; set; } = String.Empty;

        [DataMember(Name = "issueType")]
        public string IssueType { get; set; } = String.Empty;

        [DataMember(Name = "blList")]
        public string BlList { get; set; } = String.Empty;

        [DataMember(Name = "releaseNotes")]
        public string ReleaseNotes { get; set; } = String.Empty;

        [DataMember(Name = "url")]
        public string Url { get; set; } = String.Empty;

        private static string getJiraUrl(string key)
        {
            return ConfigProvider.JiraUrl + "/browse/" + key;
        }

        private void initReleaseNotesAndBlList(string releaseNotes)
        {
            ReleaseNotes = releaseNotes.Replace("<", "'");
            ReleaseNotes = ReleaseNotes.Replace(">", "'");

            string upperNotes = releaseNotes.ToUpper();
            string notesHeader = "REST DIFF:";
            int notesStartPos = upperNotes.IndexOf(notesHeader);
            BlList = string.Empty;

            if (notesStartPos > -1)
            {
                string notesText = releaseNotes.Substring(notesStartPos + notesHeader.Length).Trim();
                Regex blRegex = new Regex("(?<=\\<)\\w+");
                Match match = blRegex.Match(notesText);
                HashSet<string> blList = new HashSet<string>();

                while (match.Success)
                {
                    if (!blList.Contains(match.Value.ToUpper())) 
                    {
                        if (BlList != string.Empty)
                        {
                            BlList += ", ";
                        }
                        BlList += match.Value;
                        blList.Add(match.Value.ToUpper());
                    }
                    match = match.NextMatch();
                }
            }

        }
    }
}