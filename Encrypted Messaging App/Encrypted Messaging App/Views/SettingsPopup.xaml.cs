using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Extensions;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using static Encrypted_Messaging_App.Views.Functions;

namespace Encrypted_Messaging_App.Views
{   [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPopup : Rg.Plugins.Popup.Pages.PopupPage
    {   public SettingsPopup() { InitializeComponent(); }

        protected override void OnAppearing()
        {   setValues();
            base.OnAppearing();
        }
        private void setValues()
        {   Dictionary<Span, string> newLabelValues = new Dictionary<Span, string>() {
                {settingUsername, CurrentUser.Username},
                {settingChats, CurrentUser.chats.Count.ToString() },
                {settingRequests, (CurrentUser.friendRequests != null ? CurrentUser.friendRequests.Length.ToString() : "0") }
            };
               // Add text contents:
            foreach(KeyValuePair<Span, string> entry in newLabelValues)
            {
                entry.Key.Text = entry.Value;
            }  // Add themes to picker:
            for(int i=1; i< colourDict.Count; i++)
            {
                string themeName = colourDict.Keys.ToArray()[i];
                settingThemePicker.Items.Add(themeName);
            }  // Set selected colour:
            Color currentThemeColour = Color.FromHex(colourDict[CurrentTheme][0]);
            settingThemePicker.SelectedItem = CurrentTheme;
        }

        public void OnThemeChanged(object sender, EventArgs e) { 
            setColour((string)settingThemePicker.SelectedItem);
        }

        public async void OnFeedbackPressed(object sender, EventArgs e)
        {
            Button feedbackButton = (Button)sender;
            string feedbackType = feedbackButton.Text;
            if(feedbackType != "Leave Feedback") { feedbackType = "Report Bug"; }
            string feedbackMessage = await DisplayPromptAsync($"{feedbackType}:", null, maxLength:100, keyboard:Keyboard.Text);
            if(feedbackMessage == null || feedbackMessage.Length == 0) { return; }
            await DependencyService.Resolve<IManageFirestoreService>().AddToArray(feedbackMessage, $"other/feedback/{feedbackType.Split(' ')[1].ToLower()}");
            Color startColor = feedbackButton.BackgroundColor;
            Color textColor = feedbackButton.TextColor;
            double borderWidth = feedbackButton.BorderWidth;

            feedbackButton.BackgroundColor = Color.Green;
            feedbackButton.TextColor = Color.White;
            feedbackButton.BorderWidth = 0;
            await Task.Delay(2000);
            feedbackButton.BackgroundColor = startColor;
            feedbackButton.TextColor = textColor;
            feedbackButton.BorderWidth = borderWidth;
        }
    }
}