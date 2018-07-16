namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class RestService
    {
        public string Href { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Filepath { get; set; }

        public string Error { get; set; }

        public bool IsError {
            get {
                return this.Error != null;
            }
        }
        
    }
}
