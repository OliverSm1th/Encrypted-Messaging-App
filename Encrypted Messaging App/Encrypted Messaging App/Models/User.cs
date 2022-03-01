using Xamarin.Forms;
using static Encrypted_Messaging_App.LoggerService;

namespace Encrypted_Messaging_App
{
    public class User
    {
        private IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();

        public string Id { get; set; }
        public string Username { get; set; }
        public string[] chatsID { get; set; }  // Only set when initilising firstore users
        

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
    }
}
