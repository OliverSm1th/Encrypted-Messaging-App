using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Xamarin.Forms;
using System.Threading.Tasks;

using Encrypted_Messaging_App.Views;
using Encrypted_Messaging_App.Services;
using System.Linq;
using System.Reflection;

namespace Encrypted_Messaging_App
{
    /*public class Chat
    {
        public string Title { get; set; }
        public string Id;

        public List<Message> Messages { get; set; }
        public User[] Users { get; set; }

        public KeyData EncryptionInfo { get; set; }
        private BigInteger EncryptionKey;

        bool isDefined = false;


        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();

        // Creating Chat from scratch
        public Chat(KeyData encryptionInfo, BigInteger key, User[] users)
        {
            EncryptionInfo = encryptionInfo;
            EncryptionKey = key;
            Users = users;
            Title = "";

            Messages = new List<Message>();
        }
        // Defining Chat already on server
        public Chat(string chatID)
        {
            Id = chatID;
            string key = SQLiteService.ChatKeys.Get(chatID);
            if (key.Length > 0)
            {
                EncryptionKey = BigInteger.Parse(key);
            }
            else
            {
                DebugManager.ErrorSilent($"Unable to add chat {Id} to ChatKeys in Storage");
            }
        }

        public Chat() { }

        public async Task<(bool, string)> InitiliseFirestore()
        {
            if (!propertiesDefined()) { return (false, "Not all properties are defined: Invalid Chat Object"); }
            (bool success, string message) result = await FirestoreService.InitiliseChat(this); //OtherUsers.Concat(new User[] { CurrentUser}).ToArray()

            if (result.success)
            {
                Id = result.message;
                Console.WriteLine($"Set id to: {Id}");
            }

            return result;
        }

        public async Task<(bool, string)> AddToCUser(string CUserID)
        {
            (bool success, string message) result = await FirestoreService.AddChatIDToUser(CUserID, Id);
            return result;
        }
        
        public void ListenerInit()
        {
            FirestoreService.ListenData("Chat", (result) => UpdateChat((Chat) result), new Dictionary<string, string> { { "CHATID", Id } });
        }

        private void UpdateChat(Chat newChat)
        {
            Messages = newChat.Messages;
            //OtherUsers = newChat.Users.Where((User user) => user.Id != CurrentUser.Id).ToArray();
            Title = newChat.Title;
        }

        public void RemoveListeners()
        {
            FirestoreService.RemoveListeners();
        }

        private bool propertiesDefined()
        {
            bool defined = true;
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                if(prop.GetValue(this, null) == null)
                {
                    Console.WriteLine($"{prop} is not defined!!");
                    defined = false;
                }
            }
            return defined;
        }
    }*/

    public class Chat
    {
        public KeyData encryptionInfo { get; set; }
        public string title { get; set; }
        public User[] users { get; set; }
        public List<Message> messages { get; set; }
        public string Id;

        private BigInteger encryptionKey;


        IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();


        public Chat() { }

        // Constructors:
        public void CreateFromData(KeyData chatEncryptInfo, BigInteger chatEncryptKey, User[] chatUsers)
        {
            encryptionInfo = chatEncryptInfo;
            users = chatUsers;
            encryptionKey = chatEncryptKey;
            messages = new List<Message>();
            title = "";
        }

        public void GetFromServer(string chatID)
        {
            Id = chatID;
        }


        // Firestore Update/Create:
        public async Task<(bool, string)> initiliseChatFirestore()
        {
            if (!propertiesDefined()) { return (false, "Not all properties are defined: Invalid Chat Object"); }
            (bool success, string message) result = await FirestoreService.InitiliseChat(this);

            if (result.success)
            {
                Id = result.message;
                Console.WriteLine($"Set id to: {Id}");
            }

            return result;
        }

        public async Task<(bool, string)> updateTitle(string newTitle)
        {
            (bool success, string message) result = await FirestoreService.UpdateString(newTitle, FirestoreService.GetPath("Chat", arguments: ("CHATID", Id))+"/Title");
            return result;
        }


        public async Task<(bool, string)> addToUserFirestore(string CUserID)
        {
            (bool success, string message) result = await FirestoreService.AddToArray(CUserID, FirestoreService.GetPath("CUser")+"/chatsID");
            return result;
        }

        
        
        // Listeners:
        public bool initiliseListener()
        {
            if(Id != null)
            {
                FirestoreService.ListenData("Chat", (result) => updateChat((Chat)result), arguments:("CHATID", Id));  //new Dictionary<string, string> { { "CHATID", Id } }
                return true;
            }
            return false;
        }
        public void removeListener()
        {
            FirestoreService.RemoveListeners();
        }


        // Private Methods:
        private void updateChat(Chat newChat)
        {
            messages = newChat.messages;
            users = newChat.users;
            title = newChat.title;
        }
        private bool propertiesDefined()
        {
            bool defined = true;
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                if (prop.GetValue(this, null) == null)
                {
                    Console.WriteLine($"{prop} is not defined!!");
                    defined = false;
                }
            }
            return defined;
        }

        
    }
}
