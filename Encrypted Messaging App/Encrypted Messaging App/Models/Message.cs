using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App.Models
{
    class Message
    {
        public Message(string content)
        {
            content = content;
            createdTime = DateTime.Now;


        }
        public string content { get; set; }
        public DateTime createdTime { get; }
        public DateTime deliveredTime { get; }
        public DateTime readTime { get; }
        public User author;
        public User recipient;

    }
}
