using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Extensions;
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
            Label noChatLabel = (Label)Content.FindByName("NoChats");

            if (chats == null || chats.Length == 0) { noChatLabel.IsVisible = true; }
            else { noChatLabel.IsVisible = false; }
            if (chats == currentChats) {  return;  }

            currentChats = chats;

            Debug($"Displaying chats: length: {chats.Length}");

            ChatsCollection.Clear();
            if (chats == null || chats.Length == 0)  { return; }


            for (int i = 0; i < chats.Length; i++)
            {
                ChatsCollection.Add(chats[i]);
                Debug($"{chats[i].id} chat has been added to list");
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

        
        public Command MessageRefreshCommand
        {
            get
            {  
                return _messageRefreshCommand ?? (_messageRefreshCommand = new Command(() =>
                {
                    Refresh();
                }));
            }
        } // From: https://xamarinmonkeys.blogspot.com/2020/01/xamarinforms-working-with-refreshview.html
        private Command _messageRefreshCommand;

        public void Refresh()
        {
            CurrentUser.Output();
            DisplayChats(CurrentUser.chats.ToArray());
            RefreshView messageRefresh = (RefreshView)Content.FindByName("MessageRefresh");
            messageRefresh.IsRefreshing = false;
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



        

        public void SettingsPopOut(object sender, EventArgs e)
        {
            Navigation.PushPopupAsync(new SettingsPopup());
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

        // Depracted: 
        public async void ChangeThemePopOut(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Change Theme:", "Cancel", null, "🟦 Blue", "🟥 Red");
            if (action == null || action == "Cancel") { return; }
            Functions.setColour(action.Split(' ')[1]);
        }
    }
}