using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App
{
    public class Message
    {
        public Message(string p_content, User p_author)
        {
            content = p_content;
            createdTime = DateTime.Now;
            author = p_author;


        }

        public Message() { }
        public string content { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime deliveredTime { get; set; }
        public DateTime readTime { get; set; }
        public User author { get; set; }

    }
}
