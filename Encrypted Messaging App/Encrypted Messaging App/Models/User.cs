using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App
{
    public class User
    {

        public string Id { get; set; }
        public string Username { get; set; }

        public User() { }
        public User(string id)
        {
            Id = id;
            Username = "unassigned";
        }
        public User(string id, string username)
        {
            Id = id;
            Username = username;
        }

        public override string ToString()
        {
            return Username;
        }


    }
}
