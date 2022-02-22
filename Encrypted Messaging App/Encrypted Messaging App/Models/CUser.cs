using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using static Encrypted_Messaging_App.LoggerService;


namespace Encrypted_Messaging_App
{
    public class CUser : User   // The Current User for the device, extends User
    {
        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>(); // Initilise Firetore Service

        User user;



        public CUser(string id, string username) : base(id, username) 
        {
            user = new User(id, username);

            // Initilises listeners to update lists when recieved:
            PendingRequestListenerInit();
            AcceptedRequestListenerInit();
            ChatsIDListenerInit();
        }





        //   --- Chats ---   \\

        public List<Chat> chats = new List<Chat>();
        public Action<object, int> chatsChangedAction;  //chats, index (optional)
        private async Task addChat(string chatID)
        {
            if (chats.Where(chat => chat.id == chatID).ToArray().Length != 0)
            {
                Error($"Can't add Chat {chatID} to Chats: Already Exists");
            }

            Chat newChat = new Chat();
            newChat.SetID(chatID);
            bool result = await newChat.FetchAndListen();
            if (result) {
                chats.Add(newChat);

                // Add listener to trigger chatsChangedAction when the new chat's edited
                newChat.headerChangedAction = () => chatsChangedAction(chats.ToArray(), chats.IndexOf((Chat)newChat));
            }
        }
        private void removeChat(string chatID)
        {
            int removedNum = chats.RemoveAll(chat => { if (chat.id == chatID) { chat.RemoveListener();} return (chat.id == chatID); });
            if (removedNum == 0) { Error($"Can't remove Chat {chatID} from Chats: Doesn't exist"); }
        }
        


        public string[] chatsID
        {
            get { return _chatsID; }
            set
            {
                // Syncs the chats to chatID, adds or removes chats accordingly
                chatsIDSet(value);
            }
        }
        private string[] _chatsID = new string[0];
        private async void chatsIDSet(string[] newChatsID)
        {
            string[] addedChatsID = newChatsID.Except(_chatsID).ToArray();
            string[] removedChatsID = _chatsID.Except(newChatsID).ToArray();

            if(addedChatsID.Length == 0 && removedChatsID.Length == 0) { return; }

            _chatsID = newChatsID;

            foreach (string chatID in removedChatsID)
            {
                removeChat(chatID);
            }
            foreach (string chatID in addedChatsID)
            {
                await addChat(chatID);
            }

            if ((addedChatsID.Length > 0 || removedChatsID.Length > 0) && chatsChangedAction != null)
            {
                chatsChangedAction(chats.ToArray(), -1);
            }
        }

        public void ChatsIDListenerInit()
        {
            FirestoreService.ListenData<string[]>("ChatsID", (newchatsID) => chatsID = (string[])newchatsID);
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
        private Request[] _friendRequests = new Request[0];
        public Action<object> friendRequestAction; // When friendRequests is changed


        public void PendingRequestListenerInit()          // When Requests (Server) is changed -> Update friendRequests
        {
            FirestoreService.ListenData<Request[]>("Requests", (requests) => friendRequests = (Request[])requests);
        }

        public async Task<bool> FetchFriendRequests()  // DEBUG: Refresh button
        {
            (bool success, object result) response = await FirestoreService.FetchData<Request[]>("Requests");
            
            if (!response.success)
            {
                if(response.result is string errorMessage && errorMessage.StartsWith("No docs found"))
                {
                    friendRequests = null;
                    return true;
                }
                else
                {
                    Error($"Can`'t fetch request: {response.result}");
                    return false;
                }
                
            }
            else
            {
                friendRequests = (Request[])response.result;
                return true;
            }
        }


        //   --- Accepted Friend Request ---
        public void AcceptedRequestListenerInit()
        {   // When AcceptRequests (Server) is changed -> Handle in AcceptRequestHandler
            FirestoreService.ListenData<AcceptedRequest[]>("AcceptRequests", AcceptRequestHandler);
        }
        private void AcceptRequestHandler(object requests)
        {   // Creates new local chat and deletes AcceptRequest
            if (requests == null) { return; }
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
                chat.RemoveListener();
            }
        }


        //   Other useful functions:
        public void Output()
        {
            Debug($"{Username} info: ");
            Debug($"ID: {Id}", 1);


            string chatIDMsg = "";
            if (chatsID != null && chatsID.Length != 0)
            {
                foreach (string chatID in chatsID)
                {
                    chatIDMsg += chatID + ", ";
                }
                Debug($"     Chat ID's: {chatIDMsg.Trim().Trim(',')}", 1);
            }
            else { Debug("     Chat ID's:  None", 1); }

            if(chats != null && chats.Count != 0)
            {
                Debug("     Chats: ");
                foreach (Chat chat in chats)
                {
                    if (chat.title == null) { Error($"Chat title not defined for: {chat.id}"); }
                    else if (chat.messages == null) { Error($"Chat Messages not defined for: {chat.id}"); }
                    else if (chat.userIDs == null) { Error($"Chat UserIDs not defined for: {chat.id}"); }
                    else { Debug($"          title:{chat.title}, id:{chat.id}, msgs:{chat.messages.Length} usrs:{chat.userIDs.Length}"); }
                }
            }
            else { Debug("     Chats:  None"); }





            string requestMsg = "";
            if (friendRequests != null && friendRequests.Length != 0)
            {
                foreach (Request request in friendRequests)
                {
                    requestMsg += request.SourceUser.Id + ",";
                }
                Debug($"     Pending Friend Requests: {requestMsg.Trim().Trim(',')}", 1);
            }
            else { Debug("     Pending Friend Requests: None", 1); }
        }
        public User GetUser()
        {
            return user;
        }
    }
}
