using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Encrypted_Messaging_App
{
    public interface IManageFirestoreService
    {
        Task<(bool, string)> InitiliseUser(string Username);

        Task<(bool, string)> InitiliseChat(Chat chat);

        Task<(bool, string)> SendAcceptedRequest(string requestUserID, AcceptedRequest ARequest);
        Task<(bool, string)> SendRequest(Request request, string requestUserID);

        Task<(bool, string)> AddToArray(string newString, string pathInfo, params (string, string)[] arguments);
        Task<(bool, string)> RemoveFromArray(string oldItem, string pathInfo, params (string, string)[] arguments);

        Task<(bool, string)> UpdateString(string newString, string pathInfo, params (string, string)[] arguments);
        Task<(bool, string)> AddMessageToChat(Message message, string chatID);


        Task<(bool, object)> FetchData<ReturnType>(string pathInfo, params (string, string)[] arguments);

        bool ListenData<ReturnType>(string type, Action<object> action, string changeType = null,string listenerKey = "", params (string, string)[] arguments);
        Task<bool> ListenDataAsync<returnType>(string pathInfo, Action<object> action, string changeType = null, bool returnOnInitial = true, string listenerKey = "", params (string, string)[] arguments);

        Task<(bool, string)> WriteObject(object obj, string pathInfo, params (string, string)[] arguments);

        Task<(bool, string)> DeleteObject(string pathInfo, params (string, string)[] arguments);



        Task<User> UserFromId(string id);
        Task<User> UserFromUsername(string username);

        void RemoveAllListeners();
        void RemoveListenersByKey(string key);
    }
}
