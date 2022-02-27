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

        }
        public Message() { }    // Defining message from server

        // Getters:
        public MessageView GetMessageView()
        {
            return new MessageView { content = content, encryptedContent = encryptedContent, author = author };
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
