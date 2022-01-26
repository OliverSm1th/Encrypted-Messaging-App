using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.LoggerService;
using static Encrypted_Messaging_App.Views.GlobalVariables;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMessagePage : ContentPage
    {
        public ObservableCollection<Chat> ChatsCollection { get; } = new ObservableCollection<Chat>();

        public MainMessagePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("~~ Main Message Page ~~");

            Chat[] chats = CurrentUser.chats.ToArray();

            DisplayChats(CurrentUser.chats.ToArray());
            
            CurrentUser.chatsChangedAction = (newChats, index) => { if (index == -1) { DisplayChats((Chat[])newChats); } else { UpdateChats((Chat[])newChats, index); } };
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        Chat[] currentChats;
        private void DisplayChats(Chat[] chats)
        {
           
            if (chats == currentChats) { return; }
            else { currentChats = chats; }
            LoggerService.Log($"Displaying chats: length: {chats.Length}");

            Label noChatLabel = (Label)Content.FindByName("NoChats");

            ChatsCollection.Clear();

            if (chats != null && chats.Length > 0)
            {
                for (int i = 0; i < chats.Length; i++)
                {
                    ChatsCollection.Add(chats[i]);
                    Console.WriteLine($"{chats[i].id} chat has been added to list");
                }
                noChatLabel.IsVisible = false;
            }
            else
            {
                noChatLabel.IsVisible = true;
            }
        }

        private void UpdateChats(Chat[] chats, int indexToChange)
        {
            if(chats.Length > indexToChange && ChatsCollection.Count == chats.Length)
            {
                ChatsCollection[indexToChange] = chats[indexToChange];
            }
            else {
                Error($"Invalid parameters (chats:{chats.Length}, collection: {ChatsCollection.Count}, index: {indexToChange})");
            }
        }

        public void Refresh(object sender, EventArgs e)
        {
            Log("Refreshing");
            CurrentUser.Output();
            DisplayChats(CurrentUser.chats.ToArray());
        }


        public void ChatTapped(object sender, EventArgs e)
        {
            Grid chatGrid = (Grid)sender;
            Console.WriteLine($"Tapped: {chatGrid.ClassId}");
            foreach (Chat chat in currentChats){
                if (chat.id == chatGrid.ClassId)
                {
                    CurrentChat = chat;
                }
            }

            Shell.Current.GoToAsync($"{nameof(ChatPage)}");
        }



        public async void ChangeThemePopOut(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Change Theme:", "Cancel", null, "🟦 Blue", "🟥 Red");
            if(action == null || action == "Cancel") { return; }
            Functions.setColour(action.Split(' ')[1]);
        }


        public void LogOut(object sender, EventArgs e)
        {
            IAuthenticationService authenticationService = DependencyService.Resolve<IAuthenticationService>();
            bool success = authenticationService.LogOut();
            if (success)
            {
                CurrentUser.LogOut();
                Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        }
    }
}