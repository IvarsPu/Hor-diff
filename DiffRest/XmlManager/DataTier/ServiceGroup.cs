using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTier
{
    public class ServiceGroup
    {
        private List<Service> services;
        private List<ServiceGroup> ServiceGroups;

        public ServiceGroup(string name)
        {
            Name = name;
            services = new List<Service>();
            ServiceGroups = new List<ServiceGroup>();
        }

        public string Name { get; set; }

        public void AddService(Service service)
        {
            services.Add(service);
        }

        public List<Service> GetServices()
        {
            return services;
        }

        public void AddServiceGroup(ServiceGroup serviceGroup)
        {
            ServiceGroups.Add(serviceGroup);
        }

        public List<ServiceGroup> GetServiceGroups()
        {
            return ServiceGroups;
        }
    }
}
