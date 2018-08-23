using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RestLogService.Models
{
    [Serializable]
    [DataContract(Name = "restVersion")]
    public class RestVersion : RestVersionBase
    {
        public RestVersion(string name, string svnUrl, string firstRevision) : base(name)
        {
            SvnUrl = svnUrl;
            FirstRevision = firstRevision;
        }
        
        [DataMember(Name = "svnUrl")]
        public string SvnUrl { get; set; }

        [DataMember(Name = "firstRevision")]
        public string FirstRevision { get; set; }
    }   
}