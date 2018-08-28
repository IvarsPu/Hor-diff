using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DiffRest.Models
{
    public class Service
    {
        public Service(string name, string serviceName, string state)
        {
            Name = name;
            ServiceName = serviceName;
            State = state;
        }

        public string Name { get; set; }

        public string ServiceName { get; set; }

        public string State { get; set; }
    }
}