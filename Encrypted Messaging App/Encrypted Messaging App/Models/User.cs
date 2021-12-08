using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static Encrypted_Messaging_App.LoggerService;

namespace Encrypted_Messaging_App
{
    public class User
    {
        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();

        public string Id { get; set; }
        public string Username { get; set; }

        public User() { }
        public User(string id)
        {
            Id = id;
            Username = "";
        }
        public User(string id, string username)
        {
            Id = id;
            Username = username;
        }

        public override string ToString()
        {
            return Username;
        }

        public async void GetFromServer()
        {
            (bool success, object user) user_result;
            if (Username != null && Username.Length > 0)
            {
                user_result = await FirestoreService.FetchData<User>("UserFromUsername", ("USERNAME", Username));
                
            }
            else if (Id != null && Id.Length > 0)
            {
                user_result = await FirestoreService.FetchData<User>("UserFromId", ("USERID", Id));
            }
            else
            {
                Error("Can't get from server, both Id and Username is null");
                return;
            }

            if (user_result.success && user_result.user is User result)
            {
                Id = result.Id;
                Username = result.Username;
            }
            else
            {
                Error($"Can't get from server: {user_result.user}");
            }
        }


    }
}
