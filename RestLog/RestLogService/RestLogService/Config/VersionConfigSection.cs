using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace RestLogService.Config
{
    public class VersionConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public VersionConfigInstanceCollection Instances
        {
            get { return (VersionConfigInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }
    public class VersionConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new VersionConfigInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            //set to whatever Element Property you want to use for a key
            return ((VersionConfigInstanceElement)element).Name;
        }
    }

    public class VersionConfigInstanceElement : ConfigurationElement
    {
        //Make sure to set IsKey=true for property exposed as the GetElementKey above
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("svnUrl", IsRequired = false)]
        public string SvnUrl
        {
            get { return (string)base["svnUrl"]; }
            set { base["svnUrl"] = value; }
        }

        [ConfigurationProperty("firstRevision", IsRequired = false)]
        public string FirstRevision
        {
            get { return (string)base["firstRevision"]; }
            set { base["firstRevision"] = value; }
        }
    }
}