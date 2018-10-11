using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class AppContext
    {
        public AppContext(string rootUrl, string username, string password, string rootLocalPath)
        {
            this.RootUrl = rootUrl;
            this.Username = username;
            this.Password = password;
            this.RootLocalPath = rootLocalPath;
        }

        public string RootUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string RootLocalPath { get; set; }

        public string ReleaseLocalPath { get; set; }

        public string Version { get; set; }

        public string Release { get; set; }

        public string MetaFilePath { get; set; }
    }
}