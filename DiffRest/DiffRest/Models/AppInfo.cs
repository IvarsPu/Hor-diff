using System.Collections.Generic;

namespace Models
{
    public class AppInfo
    {
        public static SortedDictionary<int, Process> Processes { get; set; } = new SortedDictionary<int, Process>();
        public static string path;
        public static string FolderLocation, MetadataRootFolder;
        public static readonly string JsonTreeFileName = "tree_data.js", HtmlRootFolder = "REST_DIFF";
    }
}
