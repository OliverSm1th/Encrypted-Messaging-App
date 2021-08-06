using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgotPasswordPage : ContentPage
    {
        public string primary = "#2196F3";
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private void EmailFocused(object sender, FocusEventArgs e)
        {
            Label EmailIcon = (Label)Content.FindByName("EmailIcon");
            Color newColor = Color.FromHex("#2196F3"); // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (EmailIcon.TextColor == newColor) { EmailIcon.TextColor = defaultColor; }
            else { EmailIcon.TextColor = newColor; }
        }
        private void EditedEntry(object sender, EventArgs e)
        {
            Button LoginBtn = (Button)Content.FindByName("SubmitButton");
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            if (!String.IsNullOrEmpty(EmailEntry.Text) && EmailEntry.Text.Length > 0)
            {
                LoginBtn.BackgroundColor = Color.FromHex("#2196F3"); //Primary
            }
            else
            {
                LoginBtn.BackgroundColor = Color.FromHex("#FF778899"); //Gray
            }
        }

        private async void SubmitButton_Clicked(object sender, EventArgs e)
        {
            // Send Email:
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            string email = EmailEntry.Text;
            IAuthenticationService AuthenticationService = DependencyService.Resolve<IAuthenticationService>();
            bool result = await AuthenticationService.ForgotPassword(email);
            if (result)
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert("Password Reset", "Password recovery sent, check your email", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Error!!");
            }
        }

    }
}