using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.Views.Functions;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        bool invalidEmail = false;
        Label invalidEmailIcon = new Label();
        

        public LoginPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()  // When the login page appears, if they're already logged in, they could go direct to the main page
        {
            base.OnAppearing();

            IAuthenticationService authenticationService = DependencyService.Resolve<IAuthenticationService>();
            if (authenticationService.isSignedIn())
            {
                await Shell.Current.GoToAsync($"//{nameof(LoadingPage)}");
            }
        }
        // Login
        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            Entry UsernameEntry = (Entry)Content.FindByName("UsernameEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            IAuthenticationService authenticationService = DependencyService.Resolve<IAuthenticationService>();
            if(authenticationService == null){
                Console.WriteLine("Not resolved");
                return;
            }
            var resultObj = await authenticationService.LogIn(username, password);
            bool result = resultObj.Item1;
            if (result)
            {
                await Shell.Current.GoToAsync($"//{nameof(LoadingPage)}");
            }
            else
            {

                Label InvalidMsg = (Label)Content.FindByName("InvalidLabel");
                Button LoginBtn = (Button)sender;

                string type = resultObj.Item2;
                if(type == "emailOrPassword")
                {
                    InvalidMsg.IsVisible = true;
                }
                else if(type != "")
                {
                    Label InvalidIcon = (Label)Content.FindByName($"{type}Icon");
                    IconInvalid(InvalidIcon);
                }
                LoginBtn.BackgroundColor = Color.FromHex("#E74C3C");
                LoginBtn.IsEnabled = false;
                
                
            }


            
        }


        // Changes screens- Register/Forgot Password
        private async void Register_Tap(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(RegisterPage)}");
        }

        private async void ForgotPassword_Tap(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(ForgotPasswordPage)}");
        }

        // Password visibility
        private void ChangeVisibility(object sender, EventArgs e)
        {
            Console.WriteLine("Clicked");
            Label lblClicked = (Label)sender;
            Entry passwordEntry = (Entry) Content.FindByName("PasswordEntry");
            if (lblClicked.Text == "&#xf070;" || lblClicked.Text == "\uf070")
            {
                lblClicked.Text = "\uf06e ";
                Console.WriteLine("Changed to shown");
                passwordEntry.IsPassword = false;
            }
            else
            {
                lblClicked.Text = "\uf070";
                Console.WriteLine($"Changed to hidden");
                passwordEntry.IsPassword = true;
            }
            lblClicked.FontFamily = "Icons-R";
        }


        // OnEdited- When any entry is edited, check if all entries have content and light up login button
        private void EditedEntry(object sender, EventArgs e)
        {
            Button LoginBtn = (Button)Content.FindByName("LoginButton");
            Entry UsernameEntry = (Entry)Content.FindByName("UsernameEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            String[] texts = new string[] { UsernameEntry.Text, PasswordEntry.Text };
            bool filled = true;
            foreach (string text in texts)
            {
                if (String.IsNullOrEmpty(text)) { filled = false; }
                else if (text.Length == 0) { filled = false; }
            }
            //bool emailValid = isValidEmail(UsernameEntry.Text);
            //Console.WriteLine($"Email: {UsernameEntry.Text}");


            if (filled) // && emailValid
            {
                if(LoginBtn.BackgroundColor == Color.FromHex("#E74C3C"))
                {
                    Label InvalidMsg = (Label)Content.FindByName("InvalidLabel");
                    InvalidMsg.IsVisible = false;
                }
                LoginBtn.BackgroundColor = Color.FromHex("#2196F3"); //Primary
                LoginBtn.IsEnabled = true;
            }
            else
            {
                LoginBtn.BackgroundColor = Color.FromHex("#FF778899"); //Gray
                LoginBtn.IsEnabled = false;
            }
            /*if (!emailValid)
            {
                EntryInvalid(EmailEntry, invalidEmailIcon, 2);
                invalidEmail = true;
            }
            else if (invalidEmail)
            {
                EntryInvalidReset(EmailEntry, invalidEmailIcon);
                invalidEmail = false;
            }*/
        }


        // Focused- When highlighted, the icon changes colour:
        private void UsernameFocused(object sender, FocusEventArgs e)
        {
            Label UserIcon = (Label)Content.FindByName("UsernameIcon");
            Color newColor = Color.FromHex("#2196F3"); // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (UserIcon.TextColor == newColor) { UserIcon.TextColor = defaultColor; }
            else { UserIcon.TextColor = newColor; } 
        }
        private void PasswordFocused(object sender, FocusEventArgs e)
        {
            Label PasswordIcon = (Label)Content.FindByName("PasswordIcon");
            Color newColor = Color.FromHex("#2196F3"); // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (PasswordIcon.TextColor == newColor) { PasswordIcon.TextColor = defaultColor; }
            else { PasswordIcon.TextColor = newColor; }
        }

        // When Entry is invalid:
    }
}