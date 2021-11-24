using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App
{
    public class Message
    {
        public Message(string p_content)
        {
            content = p_content;
            createdTime = DateTime.Now;


        }
        public string content { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime deliveredTime { get; set; }
        public DateTime readTime { get; set; }
        public User author { get; set; }
        public User recipient { get; set; }

    }
}
