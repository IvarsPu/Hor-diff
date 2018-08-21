using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTier
{
    public class Service
    {
        private List<MappedValue> myList;

        public Service(string description, string name)
        {
            Name = name;
            Description = description;
            myList = new List<MappedValue>();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public void AddServiceProperty(MappedValue mapped)
        {
            myList.Add(mapped);
        }

        public List<MappedValue> GetServiceProperties()
        {
            return myList;
        }
    }

    public class MappedValue
    {
        public MappedValue(string name, string hashCode, string noNamspaceHashCode)
        {
            Name = name;
            //Stored_release = stored_release;
            HashCode = hashCode;
            NoNamspaceHashCode = noNamspaceHashCode;
        }

        public string Name { get; set; }
        //public string Stored_release { get; set; }
        public string HashCode { get; set; }
        public string NoNamspaceHashCode { get; set; }
    }
}
