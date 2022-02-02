using System;
using System.Collections.Generic;
using System.Text;
using UsefulExtensions;
using Encrypted_Messaging_App.Encryption;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using System.Numerics;

namespace Encrypted_Messaging_App
{
    public class Message
    {
        public Message(string p_content, User p_author, string[] chatUserIDs, BigInteger p_secretKey)    // Creating the message
        {
            content = p_content;
            createdTime = DateTime.Now;
            author = p_author;
            secretKey = p_secretKey;

            string[] otherUserIDs = chatUserIDs.Remove(p_author.Id); // From Extensions
            if (otherUserIDs != null) { addEvent(PendingEventTypes.CREATED, otherUserIDs); }
        }

        public Message() { }   // Defining the message from the server
        public string content { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime deliveredTime { get; set; }
        public DateTime readTime { get; set; }
        public User author { get; set; }
        private BigInteger secretKey;


        List<MessagePendingEvent> _pendingEvents = new List<MessagePendingEvent>();
        public MessagePendingEvent[] pendingEvents
        {
            get { return _pendingEvents.ToArray(); }
            set
            {
                _pendingEvents = new List<MessagePendingEvent>(value);
                // For setting, add/subtract to _pendingEvents
            }
        }


        public bool edit(string p_newContent, User p_editor, string[] chatUserIDs)
        {
            if(author == null || author != p_editor) { return false; }

            string[] otherUserIDs = chatUserIDs.Remove(p_editor.Id); // From Extensions
            if(otherUserIDs == null) { return false; }

            return addEvent(PendingEventTypes.EDITED, otherUserIDs);
        }
        public bool delete(User p_deleter, string[] chatUserIDs)
        {
            if(author == null || author != p_deleter) { return false; }

            string[] otherUserIDs = chatUserIDs.Remove(p_deleter.Id); // From Extensions
            if (otherUserIDs == null) { return false; }

            return addEvent(PendingEventTypes.DELETED, otherUserIDs);
        }



        private bool addEvent(string eventType, string[] pendingUserIDs)
        {
            if (!PendingEventTypes.isValidEventType(eventType)) { return false; }

            _pendingEvents.Add(new MessagePendingEvent { eventType = eventType, pendingUserIDs = pendingUserIDs });
            return true;
        }

        private string encryptContent(string content)
        {
            AES encryptAES = new AES(SecurityLevel);
            byte[] byteEncrypted = encryptAES.Encrypt(secretKey.ToByteArray(), content);
            string encryptedContent = Convert.ToBase64String(byteEncrypted);
            return encryptedContent;
        }
    }

    public class MessagePendingEvent
    {
        public MessagePendingEvent() {}

        public string eventType { get; set; }
        public string[] pendingUserIDs { get; set; }

    }



    public static class PendingEventTypes
    {
        
        public static string CREATED { get; } = "Created";
        public static string DELETED { get; } = "Deleted";
        public static string EDITED { get; } = "Edited";

        private static string[] validEventTypes = new string[] { "Created", "Deleted", "Edited" };

        public static bool isValidEventType(string eventType)
        {
            return Array.IndexOf(validEventTypes, eventType) != -1;
        }
    }

}
