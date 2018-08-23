﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RestLogService.Models
{
    [Serializable]
    [DataContract(Name = "restVersion")]
    public class RestVersionBase
    {
        public RestVersionBase(string name)
        {
            Name = name;
        }
        
        [DataMember(Name = "name")]
        public string Name { get; set; }

     
    }   
}