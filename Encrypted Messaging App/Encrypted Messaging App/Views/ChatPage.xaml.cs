using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Extensions;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.LoggerService;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        // This uses MessageView instead of Message so that I can combine content and encryptedContent to visibleContent,
        // so I can change it when you disable/enable decryption (can't be done otherwise)
        public ObservableCollection<MessageView> MessagesCollection { get; } = new ObservableCollection<MessageView>();
        

        public ChatPage() {  InitializeComponent();  BindingContext = this; }

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

        private void DisplayMessages()
        {
            MessagesCollection.Clear();

            for (int i = 0; i < CurrentChat.messages.Length; i++)
            {
                MessagesCollection.Add(CurrentChat.messages[i].GetMessageView());
            }
        }

        private void UpdateHeaders() {
            Title = CurrentChat.title;
        }

        

        private void UpdateMessages(int[] editedIndex)  
        {
            if(editedIndex == null) { DisplayMessages(); }

            foreach(int index in editedIndex)
            {
                if(index < MessagesCollection.Count)
                {
                    MessagesCollection[index] = CurrentChat.messages[index].GetMessageView();
                } else {
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
            else { Log("Sent message"); }
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

        public void DisplayChatInfo(object sender, EventArgs e)
        {
            Action decryptChanged = DisplayMessages;
            Functions.OutputProperties(CurrentChat);
            Navigation.PushPopupAsync(new ChatPopup(decryptChanged));
        }
    }
}