using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class AppContext
    {
        public AppContext(RestConnection profile, string rootLocalPath, Object logger)
        {
            this.RootUrl = profile.Url;
            this.Username = profile.Username;
            this.Password = profile.Password;
            this.RootLocalPath = rootLocalPath;
            this.Logger = logger;
            this.LoadQuery = profile.LoadQuery;
            this.LoadTemplate = profile.LoadTemplate;
            this.ParallelThreads = profile.ParallelThreads;
        }

        public string RootUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string RootLocalPath { get; set; }

        public string ReleaseLocalPath { get; set; }

        public string Version { get; set; }

        public string Release { get; set; }

        public string MetaFilePath { get; set; }

        public Object Logger { get; set; }

        public bool LoadQuery { get; set; }

        public bool LoadTemplate { get; set; }

        public int ParallelThreads { get; set; } = 2;
    }
}