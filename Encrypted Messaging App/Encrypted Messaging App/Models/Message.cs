using System;
using System.Collections.Generic;
using System.Text;
using UsefulExtensions;
using Encrypted_Messaging_App.Encryption;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.LoggerService;  // Error()
using System.Numerics;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace Encrypted_Messaging_App
{
    public class Message
    {
        IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();  // Initilise Firetore Service

        // Public Attributes:  (Firebase)
        public DateTime createdTime { get; set; }
        public User author { get; set; }
        public MessageUserEvents userEvents { get; set; } = new MessageUserEvents();
        public MessagePendingEvent[] pendingEvents
        {
            get { return _pendingEvents.ToArray(); }
            set
            {
                _pendingEvents = new List<MessagePendingEvent>(value);
                // For setting, add/subtract to _pendingEvents
            }
        }
        private List<MessagePendingEvent> _pendingEvents = new List<MessagePendingEvent>();
        public string encryptedContent
        {
            get => _encryptedcontent;
            set { _encryptedcontent = value;
                if (content == null && secretKey != default) { 
                    DecryptContent(); 
                } }
        }
        private string _encryptedcontent;
        //                      (Non-Firebase)
        public string content;
        public BigInteger secretKey;
        public Action messageChangedAction;
        public int index = -1;


        // Constructors:
        public Message(string p_content, User p_author, string[] chatUserIDs, BigInteger p_secretKey)    // Creating the message
        {
            content = p_content;
            createdTime = DateTime.Now;
            author = p_author;
            secretKey = p_secretKey;

            string[] otherUserIDs = chatUserIDs.Remove(p_author.Id); // From Extensions
            if (otherUserIDs != null) { addEvent(PendingEventTypes.CREATED, otherUserIDs); }
        }
        public Message() { }    // Defining message from server

        // Getters:
        public MessageView GetMessageView()
        {
            return new MessageView { content = content, encryptedContent = encryptedContent, author = author };
        }




        public void AckDelivery(string chatID) // Acknoweledge that the message has been delivered
        {
            if(index == -1) { Error("Invalid message object (not server instance)"); return; }
            userEvents.AddEvent(chatID, index, MessageEventTypes.DELIVERED);

        }
        public void AckRead(string chatID)     // Acknowledge that the message has been read
        {
            if (index == -1) { Error("Invalid message object (not server instance)"); return; }
            userEvents.AddEvent(chatID, index, MessageEventTypes.READ);
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


        //   --Encryption + Decryption--  \\
        public bool EncryptContent()
        {
            if (secretKey.IsZero || content == null || content.Length == 0) { return false; }
            AES encryptAES = new AES(SecurityLevel);
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


        //  Private Methods:
        private bool addEvent(string eventType, string[] pendingUserIDs)
        {
            if (!PendingEventTypes.isValidEventType(eventType)) { return false; }

            _pendingEvents.Add(new MessagePendingEvent { eventType = eventType, pendingUserIDs = pendingUserIDs });
            return true;
        }
        private static byte[] UnicodeToByteArr(string unicode) //Encryption Conversions
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

    // Pending Events:   (When the message is changed)
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

     // User Events:  (When a user receives/reads a message)
    public class MessageUserEvents
    {
        
        public MessageUserEvents() { }
        public MessageEvent[] readEvents{ 
            get { return _readEvents.ToArray(); } set { _readEvents = value.ToList(); }
        }
        private List<MessageEvent> _readEvents = new List<MessageEvent>();
        public MessageEvent[] deliveredEvents
        {
            get { return _deliveredEvents.ToArray(); }
            set { _deliveredEvents = value.ToList(); }
        }
        private List<MessageEvent> _deliveredEvents = new List<MessageEvent>();

        public void AddEvent(string chatID, int messageIndex, string eventType)
        {
            if(eventType == MessageEventTypes.DELIVERED)
            {
                _deliveredEvents.Add(new MessageEvent(CurrentUser.Id));
            }
            else if(eventType == MessageEventTypes.READ)
            {
                _readEvents.Add(new MessageEvent(CurrentUser.Id));
            }

            Error($"Invalid MessageEventType: {eventType}");
        }
    }
    public class MessageEvent
    {
        public MessageEvent() { }
        public MessageEvent(string p_userID)
        {
            userID = p_userID;
            eventTime = DateTime.Now;
        }
        public string userID { get; set; }
        public DateTime eventTime { get; set; }
    }
    public static class MessageEventTypes
    {
        public static string DELIVERED { get; } = "Delivered";
        public static string READ { get; } = "Read";
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
