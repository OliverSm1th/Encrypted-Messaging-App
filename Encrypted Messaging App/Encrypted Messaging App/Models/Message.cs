using System;
using System.Collections.Generic;
using System.Text;
using UsefulExtensions;
using Encrypted_Messaging_App.Encryption;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using System.Numerics;
using Xamarin.Forms;

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
        public string content;

        string _encryptedcontent;
        public string encryptedContent { 
            get => _encryptedcontent;
            set { _encryptedcontent = value; DecryptContent(); }
        }
        public DateTime createdTime { get; set; }
        public DateTime deliveredTime { get; set; }
        public DateTime readTime { get; set; }
        public User author { get; set; }
        public BigInteger secretKey; // Set by chat when added to Message[] messages


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

        public bool EncryptContent()
        {
            if(secretKey.IsZero || content == null || content.Length == 0) { return false; }
            AES encryptAES = new AES(SecurityLevel, true);
            byte[] byteEncrypted = encryptAES.Encrypt(secretKey.ToByteArray(), UnicodeToByteArr(content));
            encryptedContent = ByteArrToBase64(byteEncrypted);
            return true;
        }

        public bool DecryptContent()
        {
            if (secretKey.IsZero || encryptedContent == null || encryptedContent.Length == 0) { return false; }
            AES decryptAES = new AES(SecurityLevel);
            byte[] byteContent = decryptAES.Decrypt(secretKey.ToByteArray(), Base64ToByteArr(encryptedContent));
            content = ByteArrToUnicode(byteContent);
            return true;
        }


        public bool Edit(string p_newContent, User p_editor, string[] chatUserIDs)
        {
            if(author == null || author != p_editor) { return false; }

            string[] otherUserIDs = chatUserIDs.Remove(p_editor.Id); // From Extensions
            if(otherUserIDs == null) { return false; }

            return addEvent(PendingEventTypes.EDITED, otherUserIDs);
        }
        public bool Delete(User p_deleter, string[] chatUserIDs)
        {
            if(author == null || author != p_deleter) { return false; }

            string[] otherUserIDs = chatUserIDs.Remove(p_deleter.Id); // From Extensions
            if (otherUserIDs == null) { return false; }

            return addEvent(PendingEventTypes.DELETED, otherUserIDs);
        }


        public MessageView GetMessageView()
        {
            Console.WriteLine($"Encrypted Content: {encryptedContent}");
            return new MessageView { content = content, encryptedContent = encryptedContent, author = author };
        }


        private bool addEvent(string eventType, string[] pendingUserIDs)
        {
            if (!PendingEventTypes.isValidEventType(eventType)) { return false; }

            _pendingEvents.Add(new MessagePendingEvent { eventType = eventType, pendingUserIDs = pendingUserIDs });
            return true;
        }

        // Encryption Conversions:
        private static byte[] UnicodeToByteArr(string unicode)
        {
            return Encoding.Unicode.GetBytes(unicode);
        }
        private static string ByteArrToUnicode(Byte[] result)
        {
            return Encoding.Unicode.GetString(result);
        }
        private static string ByteArrToBase64(Byte[] result)
        {
            return Convert.ToBase64String(result);
        }
        private static byte[] Base64ToByteArr(string base64)
        {
            return Convert.FromBase64String(base64);
        }

    }

    // Pending Events:
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

    //Message View:    (for messageList)
    public class MessageView
    {
        public MessageView() { }

        public string encryptedContent;
        public string content;
        public User author;
        string currentUserId = CurrentUser.GetUser().Id;

        public string visibleContent { get => (CurrentChat.showDecryptedMessages ? (content != null ? content : encryptedContent) : encryptedContent); }
        public LayoutOptions horizontalOption { 
            get =>  (author.Id == currentUserId ? LayoutOptions.End : LayoutOptions.Start);
        }
        public Color messageColour
        {
            get => (author.Id == currentUserId ? (Color)App.Current.Resources["MessageSent"] : (Color)App.Current.Resources["MessageReceived"]);
        }
    }

}
