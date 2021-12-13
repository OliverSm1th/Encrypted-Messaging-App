using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            
            CurrentUser.chatsChangedAction = (newChats) => DisplayChats((Chat[])newChats);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        Chat[] currentChats;
        private void DisplayChats(Chat[] chats)
        {
            LoggerService.Log($"Displaying chats: length: {chats.Length}");
            if (chats == currentChats) { return; }
            else { currentChats = chats; }

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

        public void Refresh(object sender, EventArgs e)
        {
            LoggerService.Log("Refreshing");
            CurrentUser.Output();
            DisplayChats(CurrentUser.chats.ToArray());
        }


        public void ChatTapped(object sender, EventArgs e)
        {
            Grid chatGrid = (Grid)sender;
            Console.WriteLine($"Tapped: {chatGrid.ClassId}");
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