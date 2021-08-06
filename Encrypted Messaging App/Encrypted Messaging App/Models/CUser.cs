using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App
{
    public class CUser : User
    {
        public CUser(string id, string username) : base(id, username) 
        {
            Request temp = new Request();
            temp.setSample();
            FriendRequests = new Request[] { temp };
            FriendRequestsUpdated = true;
        }


        public string[] chats { get; set; }
        public bool chatsUpdated;

        
        public Request[] FriendRequests;
        public bool FriendRequestsUpdated;

        public void SetFriendRequests()
        {
            FriendRequestsUpdated = true;
        }
        public void SetChats(string[] new_chats)
        {
            chats = new_chats;
            chatsUpdated = true;
        }

        

        public void Output()
        {
            Console.WriteLine($"ID: {Id}");
            string chatIDMsg = "";
            if (chats != null)
            {
                foreach (string chat in chats)
                {
                    chatIDMsg += chat;
                }
                Console.WriteLine($"Chat ID's: {chatIDMsg}");
            }


            string requestMsg = "";
            if (FriendRequests != null)
            {
                foreach (Request request in FriendRequests)
                {
                    requestMsg += request.userID;
                }
                Console.WriteLine($"Request User's: {chatIDMsg}");
            }
        }

    }
}
