namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class FileLoadTask
     {
        public FileLoadTask(string url, string localFolder, string filename)
        {
            this.Url = url;
            this.Filename = filename;
            this.LocalFolder = localFolder;
        }

        public string Url { get; set; }

        public string Filename { get; set; }

        public string LocalFolder { get; set; }

        public XmlData FileXmlData { get; set; }

        public string Error { get; set; }

        public bool Attachment { get; set; } = false;

    }
}
