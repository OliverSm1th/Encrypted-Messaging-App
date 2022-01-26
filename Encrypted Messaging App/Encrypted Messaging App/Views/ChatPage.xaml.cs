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

            UpdateMessages(CurrentChat.messages);


            CurrentChat.headerChangedAction += UpdateHeaders;
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

        private void UpdateMessages(Message[] messages)
        {
            MessagesCollection.Clear();

            for (int i=0; i<0; i++)
            {
                MessagesCollection.Add(messages[i]);
            }
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