using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Extensions;
using static Encrypted_Messaging_App.Views.GlobalVariables;

namespace Encrypted_Messaging_App.Views
{   [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        Action onDecryptChanged;

        public ChatPopup(Action decryptChanged)
        {   onDecryptChanged = decryptChanged; // Refreshes messageList
            InitializeComponent();  }

        protected override void OnAppearing() {  setValues(); base.OnAppearing();  }


        private void setValues()
        {   // Dictionary of text fields to updated contents
            Dictionary<Span, string> newLabelValues = new Dictionary<Span, string>() {
                {chatTitle, CurrentChat.title},
                {chatMessageNum, CurrentChat.messages.Length.ToString() },
                {chatKey, CurrentChat.GetPrivateKeyStr(SecurityLevel) },
                {chatUsers, CurrentChat.GetUsersStr() }
            };  // Populate text fields
            foreach (KeyValuePair<Span, string> entry in newLabelValues) {  
                entry.Key.Text = entry.Key.Text + entry.Value;  }

            if (!CurrentChat.showDecryptedMessages)
            {   chatDecryptionStatus.Text = "Disabled";
                chatDecryptionStatus.TextColor = (Color)App.Current.Resources["Invalid"];
            }
        }

        public void Decryption_Tap(object sender, EventArgs e)
        {   // Switch decryption status
            if (CurrentChat.showDecryptedMessages)
            {   chatDecryptionStatus.Text = "Disabled";
                chatDecryptionStatus.TextColor = (Color)App.Current.Resources["Invalid"];
                CurrentChat.showDecryptedMessages = false;
            } else {   
                chatDecryptionStatus.Text = "Enabled";
                chatDecryptionStatus.TextColor = Color.Green;
                CurrentChat.showDecryptedMessages = true;
            }
            onDecryptChanged.Invoke();  // Refresh messageList
        }
    }
}