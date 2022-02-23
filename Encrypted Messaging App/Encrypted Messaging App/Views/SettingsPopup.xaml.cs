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
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public SettingsPopup()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            setValues();
            base.OnAppearing();
        }
        private void setValues()
        {
            Dictionary<Span, string> newLabelValues = new Dictionary<Span, string>() {
                {settingUsername, CurrentUser.Username},
                {settingChats, CurrentUser.chats.Count.ToString() },
                {settingRequests, (CurrentUser.friendRequests != null ? CurrentUser.friendRequests.Length.ToString() : "0") }
            };

            foreach(KeyValuePair<Span, string> entry in newLabelValues)
            {
                entry.Key.Text = entry.Value;
            }

            // Themes:
            for(int i=1; i< colourDict.Count; i++)
            {
                string themeName = colourDict.Keys.ToArray()[i];
                settingThemePicker.Items.Add(themeName);
            }

            Color currentThemeColour = Color.FromHex(colourDict[CurrentTheme][0]);
            Console.WriteLine(currentThemeColour);
            settingThemePicker.SelectedItem = CurrentTheme;
        }

        public void OnThemeChanged(object sender, EventArgs e)
        {
            setColour((string)settingThemePicker.SelectedItem);
        }
    }
}