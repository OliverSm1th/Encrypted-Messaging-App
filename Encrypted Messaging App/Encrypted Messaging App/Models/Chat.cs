﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Xamarin.Forms;
using System.Threading.Tasks;

using Encrypted_Messaging_App.Views;
using Encrypted_Messaging_App.Services;
using System.Linq;
using System.Reflection;
using static Encrypted_Messaging_App.LoggerService;

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
        public List<User> users = new List<User>();

        
        public string[] userIDs 
        {
            get { return _userIDs; } 
            set
            {
                // Doesn't update users
                _userIDs = value;
            }
        }
        private string[] _userIDs = new string[0];

        



        public Message[] messages { get; set; }
        public string id { get; set; }

        private BigInteger encryptionKey;


        IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();


        public Chat() { }

        // Sync users with userIDs
        public async Task setUserIDsAndUsers(string[] newUserIDs)
        {
            string[] addedIDs = newUserIDs.Except(_userIDs).ToArray();
            string[] removedIDs = _userIDs.Except(newUserIDs).ToArray();
            _userIDs = newUserIDs;

            foreach (string userID in removedIDs) { users.RemoveAll(user => user.Id == userID); }
            foreach (string userID in addedIDs) { await addUser(userID); }
        }
        public async Task<bool> RefreshUsersFromIDs()
        {
            users = new List<User>();
            bool success = true;
            foreach (string userID in userIDs)
            {
                success &= await addUser(userID);
            }
            return success;
        }
        private async Task<bool> addUser(string userId)
        {
            User result = await FirestoreService.UserFromId(userId);
            if (result == null) { Error($"Unable to add user: {userId} (chat: {id})"); return false; }
            users.Add(result);
            return true;
        }



        // Constructors:
        public void CreateFromData(KeyData chatEncryptInfo, BigInteger chatEncryptKey, User[] chatUsers)
        {
            encryptionInfo = chatEncryptInfo;
            users = chatUsers.ToList();
            userIDs = new string[chatUsers.Length];
            for (int i=0; i<chatUsers.Length; i++)
            {
                userIDs[i] = chatUsers[i].Id;
            }
            encryptionKey = chatEncryptKey;
            messages = new Message[] { };
            title = "";
        }

        public void SetID(string chatID)
        {
            id = chatID;
        }
        
        
        
        // Firestore Fetch:
        public async Task<bool> FetchAndListen()
        {
            bool result = await GetFromServer();
            if (!result) { Error($"Can't get chat from server: {id}"); return false; }
            result = initiliseListener(true);
            return result;
        }



        public async Task<bool> GetFromServer()
        {
            if (id == null) { return false; }
            
            (bool success, object result)response = await FirestoreService.FetchData<Chat>("Chat", arguments: ("CHATID", id));
            if (!response.success) { return false; }

            await updateChat((Chat)response.result);
            return true;
        }
        
        
        public bool initiliseListener(bool ignoreFirst)
        {
            if(id != null)
            {
                FirestoreService.ListenData<Chat>("Chat", (result) => _ = updateChat((Chat)result), ignoreInitialEvent: ignoreFirst, arguments: ("CHATID", id));  //new Dictionary<string, string> { { "CHATID", Id } }
                return true;
            }
            return false;
        }
        public void removeListener()
        {
            FirestoreService.RemoveListeners();
        }


        // Firestore Update/Create:
        public async Task<(bool, string)> initiliseChatFirestore()
        {
            if (!propertiesDefined()) { return (false, "Not all properties are defined: Invalid Chat Object"); }
            (bool success, string message) result = await FirestoreService.InitiliseChat(this);

            if (result.success)
            {
                id = result.message;
                Console.WriteLine($"Set id to: {id}");
            }

            return result;
        }
        public async Task<(bool, string)> updateTitle(string newTitle)
        {
            (bool success, string message) result = await FirestoreService.UpdateString(newTitle, FirestoreService.GetPath("Chat", arguments: ("CHATID", id)) + "/Title");
            return result;
        }
        public async Task<(bool, string)> addToUserFirestore(string CUserID)
        {
            (bool success, string message) result = await FirestoreService.AddToArray(id, FirestoreService.GetPath("CUser") + "/chatsID");
            return result;
        }










        // Private Methods:
        private async Task updateChat(Chat newChat)
        {
            if(newChat.messages == null) { Error($"Invalid messages retrieved for: {id}"); }
            messages = newChat.messages;

            if (newChat.userIDs == null || newChat.userIDs.Length < 2) { Error($"Invalid user IDs retrieved for: {id}"); }
            else { await setUserIDsAndUsers(newChat.userIDs); }
            

            if (users == null || users.Count < 2) { Error($"Invalid users retrieved from userIDs for: {id}"); }
            else if (userIDs.Length != users.Count) { Error($"Invalid users from userID for: {id}  ({userIDs.Length} userID vs {users.Count} users)"); }
            
            


            title = newChat.title;
            if(title == null || title.Length == 0) {
                if (newChat.userIDs != null) { title = generateDefaultTitle(); }
                else { title = "  "; }
            }
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
        private string generateDefaultTitle()
        {
            string title = "";
            foreach (User user in users)
            {
                title = title + user.Username + ", ";
            }
            if(users.Count > 0)
            {
                return title.Remove(title.Length - 2);
            }
            else
            {
                return "Empty Chat";
            }
            
        }

        
    }
}
