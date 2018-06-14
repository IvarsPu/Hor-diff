namespace HtmlXmlTest
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class DownloadData
    {
        internal Task<string> ResponseTask { get; set; }

        internal string Error { get; set; } = string.Empty;
    }
}
