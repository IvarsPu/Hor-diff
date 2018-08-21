namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
