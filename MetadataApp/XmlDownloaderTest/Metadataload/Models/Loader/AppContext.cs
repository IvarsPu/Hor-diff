using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metadataload.Models
{
    internal class AppContext
    {
        public AppContext(string rootUrl, string rootLocalPath)
        {
            this.RootUrl = rootUrl;
            this.RootLocalPath = rootLocalPath;
        }

        public string RootUrl { get; set; }

        public string RootLocalPath { get; set; }

        public string ReleaseLocalPath { get; set; }

        public string Version { get; set; }

        public string Release { get; set; }

        public string MetaFilePath { get; set; }
    }
}