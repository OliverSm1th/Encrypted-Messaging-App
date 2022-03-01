using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Numerics;
using System.Collections.ObjectModel;

using Encrypted_Messaging_App.Encryption;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.Views.Functions;
using static Encrypted_Messaging_App.LoggerService;
using static Encrypted_Messaging_App.Services.SQLiteService;



namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendRequestPage : ContentPage
    {
        IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        Dictionary<string, Request> Requests = new Dictionary<string, Request>();
        Request[] currentRequests;
        bool RequestBtnEnabled = true;



        public FriendRequestPage()  {   InitializeComponent();  BindingContext = this;  }
        

        protected override void OnAppearing()
        {   base.OnAppearing();
            Console.WriteLine("~~ Friend Request Page ~~");

            DisplayRequests(CurrentUser.friendRequests);  // Display initial requests + bind to listener
            CurrentUser.friendRequestAction = (requests) => DisplayRequests((Request[])requests);
        }

        protected override void OnDisappearing()
        {   base.OnDisappearing();
            CurrentUser.friendRequestAction = null; // Disable listener
        }



        
        public async void Refresh(object sender, EventArgs e) // When refresh button is pressed
        {   bool success = await CurrentUser.FetchFriendRequests();
            if (success)
            {
                DisplayRequests(CurrentUser.friendRequests);
            }
        }
        // Display the pending requests on a grid
        private void DisplayRequests(Request[] requests)
        {
            Label noRequestLabel = (Label)Content.FindByName("NoRequests");

            if (requests == currentRequests) { noRequestLabel.Opacity = 1; return; }
            else { currentRequests = requests; }

            Users.Clear();

            if (requests != null && requests.Length > 0)
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    User user = requests[i].SourceUser;
                    if(user == null) { Error("Request user is null"); return; }

                    Users.Add(user);
                    Console.WriteLine($"{user.Username} has been added!");
                    Requests[user.Id] = requests[i];
                }  noRequestLabel.Opacity = 0; }
            else { noRequestLabel.Opacity = 1; }
        }
        // Accept:
        public async void AcceptRequest(object sender, EventArgs e)
        {
            Label acceptLabel = (Label)sender;
            string id = acceptLabel.ClassId;
            User acceptUser = null;

            // Find User
            foreach (User user in Users)
            {
                if (user.Id == id) {  acceptUser = user;  }
            }
            if (acceptUser == null) {
                toast.LongAlert("Couldn't find friend request");
                return; }

            Request request = Requests[acceptUser.Id];
            KeyData requestKeyData = request.EncryptionInfo;


            // Create new DH session + calculate secret key
            DiffieHellman userDH = new DiffieHellman(SecurityLevel);
            requestKeyData = userDH.Respond(requestKeyData);
            BigInteger sharedKey = userDH.getSharedKey(requestKeyData);

            // Create new Chat with keyData
            string[] userIDs = new string[] { request.SourceUser.Id, CurrentUser.Id };
            User[] users = new User[] { request.SourceUser, CurrentUser.GetUser() };
            Chat newChat = new Chat { userIDs = userIDs, users = users.ToList(), encryptionInfo = requestKeyData, title="" };

            // Add new Chat to firestore
            bool success = await newChat.InitiliseFirestore();
            if (!success) { ErrorToast("Unable to create new chat"); return; }

            success = await newChat.AddToUserFirestore(CurrentUser.Id);
            if (!success) { ErrorToast("Unable to create new chat"); return; }

            success = await request.Accept(newChat.id);       // Add accepted friend request obj
            if (!success) { ErrorToast("Unable to accept request"); }
            ChatKeys.Set(newChat.id, sharedKey.ToString()); // Save secret key for decryption

            success = await request.Delete();                // Delete pending friend request obj
            if (!success) { ErrorToast("Unable to delete request"); }
        }
        public void DenyRequest(object sender, EventArgs e)
        {
            Label denyLabel = (Label)sender;
            string id = denyLabel.ClassId;
            User denyUser = null;

            foreach (User user in Users)
            {
                if (user.Id == id)
                {
                    Console.WriteLine($"Denying {user.Username}");
                    denyUser = user;
                }
            }
            if(denyUser == null) { ErrorToast("Couldn't find friend request"); return; }

            Request request = Requests[denyUser.Id];
            request.Delete();
        }

        public async void SendRequest(object sender, EventArgs e) // When submit button is pressed
        {
            if (!RequestBtnEnabled) { return; }
            Entry usernameEntry = (Entry)Content.FindByName("UsernameEntry"); // Username of target user
            if(CurrentUser.Username == usernameEntry.Text) { ErrorToast("Can't send request to yourself"); SetRequestStatus("Invalid"); return; }

            (bool success, object user) user_result = await FirestoreService.FetchData<User>("UserFromUsername", ("USERNAME", usernameEntry.Text));
            if (!user_result.success) {
                toast.LongAlert($"Couldn't find user {usernameEntry.Text}");
                SetRequestStatus("Invalid");  return;  } 
            User targetUser = (User)user_result.user;

            // Check if you've already made a request:
            (bool success, object obj) result = await FirestoreService.FetchData<Request>("Requests/[CUSERID]", ("CUSERID", targetUser.Id));
            if(result.success) { ErrorToast("Request already sent"); SetRequestStatus("Invalid"); return; }

            // Set up Request Object:
            Request request = new Request();
            bool send_result = await request.Send(targetUser.Id);
            if (send_result) {  usernameEntry.Text = ""; SetRequestStatus("Valid"); }
            else {  SetRequestStatus("Invalid"); }
        }
        public async void SetRequestStatus(string statusType)
        {
            Button RequestSendButton = (Button)Content.FindByName("RequestButton");
            if (statusType == "Invalid") { RequestSendButton.TextColor = (Color)App.Current.Resources["Invalid"]; }
            else if(statusType == "Valid") { RequestSendButton.TextColor = Color.Green; }
            await Task.Delay(2000);   // After 2 seconds, reset colour
            RequestSendButton.TextColor = Color.Black;
        }

        // Check if username is valid (basic check)
        private void EditedEntry(object sender, EventArgs e)
        {
            Button RequestBtn = (Button)Content.FindByName("RequestButton");
            Entry UsernameEntry = (Entry)Content.FindByName("UsernameEntry");
            Label FriendIcon = (Label)Content.FindByName("RequestIcon");

            bool usernameValid = isValidUsername(UsernameEntry.Text);
            if (UsernameEntry.Text.Length > 0 && usernameValid)
            {// Valid: 
                RequestBtnEnabled = true;
                RequestBtn.TextColor = Color.FromHex("#2196F3");
                IconInvalidReset(FriendIcon);
            } else
            {// Invalid:
                RequestBtnEnabled = false;
                RequestBtn.TextColor = Color.FromHex("#000000");

                if (UsernameEntry.Text.Length > 0) { IconInvalid(FriendIcon); }
                else { IconInvalidReset(FriendIcon); }
            }
        }
    }
}