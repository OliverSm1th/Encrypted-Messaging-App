using Encrypted_Messaging_App.Services;
using Encrypted_Messaging_App.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Numerics;
using System.Linq;

namespace Encrypted_Messaging_App
{
    public class CUser : User   // The Current User for the device
    {
        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();

        User user;



        public CUser(string id, string username) : base(id, username) 
        {
            user = new User(id, username);

            // Initilises listeners to update lists when recieved:
            PendingRequestListenerInit();
            AcceptedRequestListenerInit();
            ChatsIDListenerInit();
        }





        //   --- Chats ---

        public List<Chat> chats = new List<Chat>();
        public Action<object> chatsChangedAction;
        private void addChat(string chatID)
        {
            if (chats.Where(chat => chat.Id == chatID).ToArray().Length == 0)
            {
                Chat newChat = new Chat();
                newChat.GetFromServer(chatID);
                newChat.initiliseListener();
                chats.Add(newChat);
            }
            else
            {
                DebugManager.ErrorSilent($"Can't add Chat {chatID} to Chats: Already Exists");
            }
        }
        private void removeChat(string chatID)
        {
            int removedNum = chats.RemoveAll(chat => { if (chat.Id == chatID) { chat.removeListener(); return true; } return false; });


            if (removedNum == 0) { DebugManager.ErrorSilent($"Can't remove Chat {chatID} from Chats: Doesn't exist"); }
        }
        

        public string[] chatsID
        {
            get { return _chatsID; }
            set
            {
                // Syncs the chats to chatID, adds or removes chats accordingly
                string[] addedChatsID = value.Except(_chatsID).ToArray();
                string[] removedChatsID = _chatsID.Except(value).ToArray();

                _chatsID = value;

                foreach (string chatID in removedChatsID)
                {
                    removeChat(chatID);
                }
                foreach (string chatID in addedChatsID)
                {
                    addChat(chatID);
                }

                if ((addedChatsID.Length > 0 || removedChatsID.Length > 0) && chatsChangedAction != null)
                {
                    chatsChangedAction(chats.ToArray());
                }
                
            }
        }
        private string[] _chatsID = new string[0];
        

        public void ChatsIDListenerInit()
        {
            FirestoreService.ListenData("ChatsID", (newchatsID) => chatsID = (string[])newchatsID);
        }


        // Initial setup:
        /*public void SetChats(string[] new_chats)
        {
            chatsID = new ObservableCollection<string>(new_chats);
            chatsIDAction = (string[] chats) => FirestoreService.UpdateChatsID(Id, chats);
            chatsID.CollectionChanged += chatIDChanged;
        }/*
        private async void chatIDChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if(e.NewItems.Count != 1) { DebugManager.ErrorSilent($"Can't add {e.NewItems.Count} chats at once"); }
                (bool success, string message) result = await FirestoreService.AddChatID(Id, (string)e.NewItems[0]);
                if (!result.success) { DebugManager.ErrorSilent($"Can't add chatID to server: {result.message}"); }
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Count != 1) { DebugManager.ErrorSilent($"Can't remove {e.OldItems.Count} chats at once"); }
                (bool success, string message) result = await FirestoreService.RemoveChatID(Id, (string)e.OldItems[0]);
                if (!result.success) { DebugManager.ErrorSilent($"Can't remove chatID from server: {result.message}"); }
            }
            else
            {
                
                DebugManager.ErrorSilent($"Not able to handle chatsID Action: {e.Action}");
            }
        }

        public void AddChat(Chat new_chat)
        {
            chats.Add(new_chat);
            chatsID.Add(new_chat.Id);
        }*/



        //   --- Pending Friend Requests (sent to user) ---

        public Request[] friendRequests { 
            get { return _friendRequests; }
            set
            {
                _friendRequests = value;

                if (friendRequestAction != null)
                {
                    friendRequestAction(value);
                }
            }
        }
        private Request[] _friendRequests;
        public Action<object> friendRequestAction; // When friendRequests is changed


        public void PendingRequestListenerInit()          // When Requests (Server) is changed -> Update friendRequests
        {
            FirestoreService.ListenData("Requests", (requests) => friendRequests = (Request[])requests);
        }

        public async Task<(bool, string)> FetchFriendRequests()  // DEBUG: Refresh button
        {
            (bool success, object result) response = await FirestoreService.FetchData("Requests");
            if (response.success)
            {
                friendRequests = (Request[])response.result;
                return (true, "");
            }
            else
            {
                return (false, (string)response.result);
            }
        }


        //   --- Accepted Friend Request ---
        public void AcceptedRequestListenerInit()   // When AcceptRequests (Server) is changed -> Handle in AcceptRequestHandler
        {
            FirestoreService.ListenData("AcceptRequests", AcceptRequestHandler);
        }
        private void AcceptRequestHandler(object requests)  // Creates new local chat and deletes AcceptRequest
        {
            AcceptedRequest[] acceptedRequests = (AcceptedRequest[])requests;
            foreach(AcceptedRequest accepted in acceptedRequests)
            {
                accepted.HandleRequest();
            }
        }

        
        public void LogOut()
        {
            // Remove All listeners:
            FirestoreService.RemoveListeners();
            foreach(Chat chat in chats)
            {
                chat.removeListener();
            }
        }


        //   Other useful functions:
        public void Output()
        {
            Console.WriteLine($"{Username} info: ");
            Console.WriteLine($"     ID: {Id}");


            string chatIDMsg = "";
            if (chatsID != null && chatsID.Length != 0)
            {
                foreach (string chat in chatsID)
                {
                    chatIDMsg += chat + ", ";
                }
                Console.WriteLine($"     Chat ID's: {chatIDMsg.Trim().Trim(',')}");
            }
            else { Console.WriteLine("     Chat ID's:  None"); }


            string requestMsg = "";
            if (friendRequests != null && friendRequests.Length != 0)
            {
                foreach (Request request in friendRequests)
                {
                    requestMsg += request.SourceUser.Id + ",";
                }
                Console.WriteLine($"     Pending Friend Requests: {requestMsg.Trim().Trim(',')}");
            }
            else { Console.WriteLine("     Pending Friend Requests: None"); }
        }
        public User GetUser()
        {
            return user;
        }
    }
}
