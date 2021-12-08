﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.ComponentModel;
using System.Numerics;

using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.Views.Functions;
using System.Collections.ObjectModel;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendRequestPage : ContentPage
    {
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        Dictionary<string, Request> Requests = new Dictionary<string, Request>();
        IToastMessage toastMessage = DependencyService.Get<IToastMessage>();

        Request[] currentRequests;
        IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();

        public FriendRequestPage()
        { 
            InitializeComponent();
            BindingContext = this;

            
        }
        bool RequestBtnEnabled = true;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("~~ Friend Request Page ~~");

            DisplayRequests(CurrentUser.friendRequests);

            // Enable listener
            CurrentUser.friendRequestAction = (requests) => DisplayRequests((Request[])requests);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Disable listener
            CurrentUser.friendRequestAction = null;
        }



        // When refresh button is pressed
        public async void Refresh(object sender, EventArgs e)
        {
            (bool, string) response = await CurrentUser.FetchFriendRequests();
            if (response.Item1)
            {
                Console.WriteLine("Updating Requests:");
                DisplayRequests(CurrentUser.friendRequests);
            }
        }

        // Display the pending requests on a grid
        private void DisplayRequests(Request[] requests)
        {
            Label noRequestLabel = (Label)Content.FindByName("NoRequests");

            if (requests == currentRequests) { noRequestLabel.IsVisible = true; return; }
            else { currentRequests = requests; }

            Console.WriteLine("Displaying Requests");
            

            Users.Clear();

            if (requests != null && requests.Length > 0)
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    User user = requests[i].SourceUser;
                    if(user == null) { DebugManager.ErrorSilent("Request user is null"); return; }

                    Users.Add(user);
                    Console.WriteLine($"{user.Username} has been added!");
                    Requests[user.Id] = requests[i];
                }
                noRequestLabel.IsVisible = false;
                Console.WriteLine("All Added!");
            }
            else
            {
                noRequestLabel.IsVisible = true;
            }
            
        }

        // Accept:
        public async void AcceptRequest(object sender, EventArgs e)
        {
            Label acceptLabel = (Label)sender;
            string id = acceptLabel.ClassId;
            User acceptUser = default(User);

            // Find User
            foreach (User user in Users)
            {
                if (user.Id == id)
                {
                    Console.WriteLine($"Accept Friend Request: {user.Username}");
                    acceptUser = user;
                }
            }
            if (acceptUser == null)
            {
                toastMessage.LongAlert("Couldn't find friend request");
                return;
            }

            //
            Request request = Requests[acceptUser.Id];
            KeyData requestKeyData = request.EncryptionInfo;


            // Create new DH session + calculate secret key
            DiffieHellman userDH = new DiffieHellman();
            requestKeyData = userDH.Respond(requestKeyData);
            BigInteger sharedKey = userDH.getSharedKey(requestKeyData);
            //PendingRequests[acceptUser.Id] = userDH;

            // Create new Chat session with keyData
            Chat newChat = new Chat();
            newChat.CreateFromData(requestKeyData, sharedKey, new User[] { request.SourceUser, CurrentUser.GetUser() });
            

            // Add new Chat to firestore
            (bool success, string message) result = await newChat.initiliseChatFirestore();
            if (!result.success) { DebugManager.ErrorToast("Unable to create new chat", $"Can't initilise chat: {result.message}"); return; }
            //newChat.initiliseListener();
            result = await newChat.addToUserFirestore(CurrentUser.Id);
            if (!result.success) { DebugManager.ErrorToast("Unable to create new chat", $"Can't add ChatID to user: {result.message}"); return; }
            // TODO: Delete chat object (another is created)


            // TODO: AcceptedRequest to UserA  with chatID   (delete Request)
            result  = await request.accept(newChat.id);
            if (!result.success) { DebugManager.ErrorToast("Unable to  request", $"Send AcceptedRequest Failed: {result.message}"); }
        }
        public void DenyRequest(object sender, EventArgs e)
        {
            Label denyLabel = (Label)sender;
            string id = denyLabel.ClassId;
            foreach (User user in Users)
            {
                if (user.Id == id)
                {
                    Console.WriteLine($"Denying {user.Username}");
                }
            }
        }

        // When the submit button is pressed to send request
        public async void SendRequest(object sender, EventArgs e)
        {
            if (!RequestBtnEnabled) { return; }
            Entry usernameEntry = (Entry)Content.FindByName("UsernameEntry"); // Username of target user


            //(bool success, object user) user_result = await DependencyService.Resolve<IManageFirestoreService>().GetUserFromUsername(usernameEntry.Text);
            Console.WriteLine($"Fetching UserID of: {usernameEntry.Text}");
            (bool success, object user) user_result = await FirestoreService.FetchData<User>("UserFromUsername", ("USERNAME", usernameEntry.Text)); //new Dictionary<string, string>{ { "USERNAME", usernameEntry.Text } }
            if (!user_result.success)
            {
                toastMessage.LongAlert($"Couldn't find user {usernameEntry.Text}");
                Console.WriteLine($"Error: Couldn't find user {usernameEntry.Text}, message: {user_result.user}");
                return;
            } 
            User targetUser = (User)user_result.user;


            // Set up Request Object:
            Request request = new Request();
            bool send_result = await request.send(targetUser.Id);

            if (send_result)
            {
                usernameEntry.Text = "";
            }
            else
            {
                IconInvalid((Button)sender);
            }
        }



        // Other GUI Improvements:
        private void EditedEntry(object sender, EventArgs e)
        {
            Button RequestBtn = (Button)Content.FindByName("RequestButton");
            Entry UsernameEntry = (Entry)Content.FindByName("UsernameEntry");
            Label FriendIcon = (Label)Content.FindByName("RequestIcon");

            bool usernameValid = isValidUsername(UsernameEntry.Text);


            //      Valid:
            if (UsernameEntry.Text.Length > 0 && usernameValid)
            { 
                RequestBtnEnabled = true;
                RequestBtn.TextColor = Color.FromHex("#2196F3");
                IconInvalidReset(FriendIcon);
            }
            //      Invalid:
            else
            { 
                RequestBtnEnabled = false;
                RequestBtn.TextColor = Color.FromHex("#000000");

                if (UsernameEntry.Text.Length > 0) { IconInvalid(FriendIcon); }
                else { IconInvalidReset(FriendIcon); }
            }
        }


    }
}