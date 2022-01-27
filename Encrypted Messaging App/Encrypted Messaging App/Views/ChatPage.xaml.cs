using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.Views.GlobalVariables;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public ObservableCollection<Message> MessagesCollection { get; } = new ObservableCollection<Message>();


        public ChatPage()
        {
            InitializeComponent();
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine($"~~ ChatPage: {CurrentChat.id} ~~");


            UpdateHeaders();

            UpdateMessages();


            CurrentChat.headerChangedAction += UpdateHeaders;

            CurrentChat.contentChangedAction += UpdateMessages;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CurrentChat.headerChangedAction -= UpdateHeaders;

            CurrentChat = null;
        }

        public void MessageTapped(object sender, EventArgs e)
        {
            Console.WriteLine("Tapped message");
        }

        private void UpdateHeaders()
        {
            Title = CurrentChat.title;
        }

        private void UpdateMessages()
        {
            MessagesCollection.Clear();

            for (int i=0; i< CurrentChat.messages.Length; i++)
            {
                MessagesCollection.Add(CurrentChat.messages[i]);
                LoggerService.Log($"Added the {i}th message: {CurrentChat.messages[i].content}");
            }
            LoggerService.Log($"Added {MessagesCollection.Count} items to the messagesCollection");
        }

        public async void MessageSent(object sender, EventArgs e)
        {
            Entry MessageEntry = (Entry)Content.FindByName("TextEntry");

            if(MessageEntry == null || MessageEntry.Text.Length == 0) { return; }
            bool result = await CurrentChat.sendMessage(new Message(MessageEntry.Text, CurrentUser.GetUser()));
            if (!result) { LoggerService.Error("Unable to send message"); }
            else { LoggerService.Log("Sent message"); }
        }
    }
}