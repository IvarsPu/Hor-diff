using System.IO;
using System.Xml.Linq;

namespace DiffRest.Models
{
    internal class XmlFile
    {
        public XmlFile(string serviceName, string url, string localPath, string filename, bool attachment = false)
        {
            this.ServiceName = serviceName;
            this.Url = url;
            this.LocalPath = localPath;
            this.Filename = filename;
            this.Attachment = attachment;
        }

        public string ServiceName { get; set; }

        public string HttpResponse { get; set; }

        public XDocument XDocument { get; set; }

        public string Url { get; set; }

        public string LocalPath { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        public string Error { get; set; } = string.Empty;

        public int HttpResultCode { get; set; } = -1;

        public bool Exists
        {
            get
            {
                return File.Exists(this.LocalPath + this.Filename);
            }
        }

        internal void LoadLocalFile()
        {
            this.XDocument = XDocument.Load(this.LocalPath + this.Filename, LoadOptions.None);
            this.HttpResponse = this.XDocument.ToString();
        }
    }
}