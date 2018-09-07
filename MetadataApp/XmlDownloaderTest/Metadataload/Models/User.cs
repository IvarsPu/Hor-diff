using System;
using System.Runtime.Serialization;

namespace Metadataload.Models
{
    public class User
    {
        public User(){}

        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string Name { get; set; }
    }
}