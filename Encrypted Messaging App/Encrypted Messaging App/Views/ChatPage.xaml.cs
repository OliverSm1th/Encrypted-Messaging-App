using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Extensions;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.LoggerService;
using static Encrypted_Messaging_App.Views.Functions;
using System.ComponentModel;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public ObservableCollection<MessageView> MessagesCollection { get; } = new ObservableCollection<MessageView>();
        // This is a list of messageView so that I can combine content and encryptedContent to visibleContent, so I can change it when you disable/enable decryption (can't be done otherwise)


        public ChatPage()
        {
            InitializeComponent();
            BindingContext = this;

        }

        protected override void OnAppearing()
        {
            Console.WriteLine($"~~ ChatPage: {CurrentChat.id} ~~");
            

            UpdateHeaders();

            DisplayMessages();



            CurrentChat.headerChangedAction += UpdateHeaders;

            CurrentChat.contentChangedAction += UpdateMessages;
            base.OnAppearing();


        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CurrentChat.headerChangedAction -= UpdateHeaders;
            CurrentChat.contentChangedAction -= UpdateMessages;

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

        private void DisplayMessages()
        {
            MessagesCollection.Clear();

            for (int i=0; i< CurrentChat.messages.Length; i++)
            {
                MessagesCollection.Add(CurrentChat.messages[i].GetMessageView());
                LoggerService.Log($"Added the {i}th message: {CurrentChat.messages[i].content}");
            }
            LoggerService.Log($"Added {MessagesCollection.Count} items to the messagesCollection");
        }


        private void UpdateMessages(int[] deletedIndex, int[] editedIndex)  
        {
            foreach(int index in deletedIndex)
            {
                MessagesCollection.Remove(MessagesCollection[index]);
            }
            foreach(int index in editedIndex)
            {
                if(index < MessagesCollection.Count)
                {
                    MessagesCollection[index] = CurrentChat.messages[index].GetMessageView();
                }
                else
                {
                    MessagesCollection.Add(CurrentChat.messages[index].GetMessageView());
                }
                
            }
        }


        public async void EditTitle(object sender, EventArgs e)
        {
            string newTitle = await DisplayPromptAsync("Enter Title:", null, "Done", null, null, CurrentChat.titleMaxLength, Keyboard.Text, CurrentChat.title);
            if(newTitle == null || newTitle == CurrentChat.title) { return; }
            Debug(CurrentChat.title);
            bool result = await CurrentChat.UpdateTitle(newTitle);
            if (!result) { ErrorToast("Unable to edit title"); }
        }
        public async void LeaveChat(object sender, EventArgs e)
        {
            bool result = await CurrentChat.Leave();
            if (!result) { ErrorToast("Unable to leave chat"); }
            await Shell.Current.GoToAsync($"//{nameof(MainMessagePage)}");
        }

        public async void SendMessage(object sender, EventArgs e)
        {
            Entry MessageEntry = (Entry)Content.FindByName("TextEntry");
            Button MessageSendButton = (Button)Content.FindByName("MessageSend");

            if(MessageEntry == null || MessageEntry.Text.Length == 0) { return; }
            MessageSendButton.IsEnabled = false;
            bool result = await CurrentChat.SendMessage(MessageEntry.Text, CurrentUser.GetUser());
            MessageSendButton.IsEnabled = true;
            if (!result) { SendingFailed(); }
            else { LoggerService.Log("Sent message"); }
            MessageEntry.Text = "";
        }

        private async void SendingFailed()
        {
            Button MessageSendButton = (Button)Content.FindByName("MessageSend");
            ErrorToast("Failed to send message");
            MessageSendButton.TextColor = (Color)App.Current.Resources["Invalid"];
            await Task.Delay(2000);
            MessageSendButton.TextColor = (Color)App.Current.Resources["Secondary"];
            Error("Unable to send message");
        }

        private Action decryptChanged; 

        public void DisplayChatInfo(object sender, EventArgs e)
        {
            decryptChanged = DisplayMessages;
            Functions.OutputProperties(CurrentChat);
            Navigation.PushPopupAsync(new ChatPopup(decryptChanged));
        }

        
        

    }
}