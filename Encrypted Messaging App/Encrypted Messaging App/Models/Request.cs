using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

using Xamarin.Forms;
using System.Threading.Tasks;

using Encrypted_Messaging_App.Encryption;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.LoggerService;
using Encrypted_Messaging_App.Services;

namespace Encrypted_Messaging_App
{
    public class KeyData
    {
        public BigInteger prime { get; set; }
        public int global { get; set; }
        public BigInteger A_Key { get; set; }
        public BigInteger B_Key { get; set; }
        public KeyData() { }
        public KeyData(BigInteger p, int g, BigInteger secret, int userNum)
        {
            prime = p;
            global = g;
            if (userNum == 0) { A_Key = secret; }
            else { B_Key = secret; }
        }
        public String output()
        {
            return $"Prime: {prime}\nGlobal: {global}\nA Key: {A_Key}\nB Key: {B_Key}";
        }
    }


    public class AcceptedRequest
    {
        // Properties
        public string newChatID { get; set; }
        public string requestUserID { get; set; }
        public KeyData EncryptionInfo { get; set; }
        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();
        // ----------

        public AcceptedRequest(string chatID, KeyData encryptionInfo)
        {
            newChatID = chatID;
            requestUserID = CurrentUser.Id;
            EncryptionInfo = encryptionInfo;
        }

        public async void HandleRequest()
        {
            User requestUser = await DependencyService.Resolve<IManageFirestoreService>().UserFromId(requestUserID);

            //chatsID.Add(accepted.newChatID);
            string privateKey = SQLiteService.PendingRequests.Get(requestUserID);

            if (privateKey.Length > 0)
            {
                SQLiteService.PendingRequests.Delete(requestUserID);

                DiffieHellman DH = new DiffieHellman(BigInteger.Parse(privateKey));
                SQLiteService.ChatKeys.Set(newChatID, DH.getSharedKey(EncryptionInfo).ToString());

                // Check the chat still exists
                (bool success, object obj) fetchResult = await FirestoreService.FetchData<Chat>("Chat", ("CHATID", newChatID));

                if (!fetchResult.success) { ErrorToast($"{requestUser.Username} deleted the chat"); return; }

                Chat newChat = new Chat();
                newChat.SetID(newChatID);
                bool result = await newChat.AddToUserFirestore(CurrentUser.Id);
                if (!result)
                {
                    ErrorToast($"Can\'t initilise chat with {requestUser.Username}");
                }
                else
                {
                    toast.ShortAlert($"{requestUser.Username} accepted you\'re friend request!");
                }

            }
            else
            {
                ErrorToast($"Can\'t initilise chat with {requestUser.Username}");
                Error($"No private key found in SQLiteService for: {requestUserID}");
            }
        }
    }




    public class Request
    {
        // Public Properties
        public User SourceUser { get; set; }
        public KeyData EncryptionInfo { get; set; }
        // ----------

        private DiffieHellman userDH = new DiffieHellman(SecurityLevel);
        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();


        // Creating new Request
        public Request()
        {
            EncryptionInfo = userDH.Initilise();
            SourceUser = CurrentUser.GetUser();
        }
        // Declaring Request from server
        public Request(KeyData encryptionInfo, User sourceUser)
        {
            EncryptionInfo = encryptionInfo;
            SourceUser = sourceUser;
        }


        // Accept Request: Respond to DH + send AcceptedRequest obj to SourceUser
        public async Task<bool> Accept(string newChatID)     
        {
            EncryptionInfo = userDH.Respond(EncryptionInfo);
            SQLiteService.ChatKeys.Set(newChatID, userDH.getSharedKey(EncryptionInfo).ToString());

            (bool success, string message) result = await FirestoreService.SendAcceptedRequest(SourceUser.Id, new AcceptedRequest(newChatID, EncryptionInfo));

            if (!result.success)
            {
                Error($"Can\'t accept request from {SourceUser.Id}:    {result.message}");
            }

            return result.success;
        }
        public async Task<bool> Delete()
        {
            (bool success, string message) result = await FirestoreService.DeleteObject("Requests");
            if (!result.success) { Error($"Failed to delete request: {result.message}"); }
            return result.success;
        }

        // Send Request:  Send Request obj to TargetUser + save DH
        public async Task<bool> Send(string targetUserID) 
        {
            (bool success, string message) result = await FirestoreService.SendRequest(this, targetUserID);

            if (!result.success) { Error($"Send Request Failed: {result.message} (Couldn't send request)"); return false; }

            bool SQLresult = SQLiteService.PendingRequests.Set(targetUserID, userDH.getPrivateKey().ToString());

            if (!SQLresult) { Error($"Send Request Failed: {result.message} (Couldn't save SQL)"); return false; }
            return true;
        }
    
    
        
    }

}
