using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.Views.Functions;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        IAuthenticationService authenticationService = DependencyService.Resolve<IAuthenticationService>();

        Label invalidEmailIcon = new Label();
        Label invalidPasswordIcon = new Label();

        public LoginPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()  // When the login page appears, if they're already logged in, they could go direct to the main page
        {
            base.OnAppearing();
            Console.WriteLine("~~ Login Page ~~");

            if (authenticationService.isSignedIn())
            {
                await Shell.Current.GoToAsync($"//{nameof(LoadingPage)}");
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Reset Entries:
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            EmailEntry.Text = "";
            PasswordEntry.Text = "";
        }

        // Login
        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            
            if(authenticationService == null){
                Console.WriteLine("Not resolved");
                return;
            }
            (bool success, string errorMsg) result = await authenticationService.LogIn(email, password);
            if (result.success)
            {
                await Shell.Current.GoToAsync($"//{nameof(LoadingPage)}");
            }
            else
            {
                Button LoginBtn = (Button)sender;

                string type = result.errorMsg;
                if(type != "")
                {
                    Label InvalidIcon = (Label)Content.FindByName($"{type}Icon");
                    IconInvalid(InvalidIcon);
                }
                LoginBtn.BackgroundColor = (Color)App.Current.Resources["Invalid"];
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
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            string[] texts = new string[] { EmailEntry.Text, PasswordEntry.Text };
            bool filled = true;
            foreach (string text in texts)
            {
                if (string.IsNullOrEmpty(text)) { filled = false; }
                else if (text.Length == 0) { filled = false; }
            }
            bool emailValid = isValidEmail(EmailEntry.Text);
            bool passwordValid = PasswordEntry.Text == null || PasswordEntry.Text.Length >= 6;


            if (filled && emailValid && passwordValid)
            {
                LoginBtn.BackgroundColor = (Color)App.Current.Resources["Primary"];
                LoginBtn.IsEnabled = true;
            }
            else
            {
                LoginBtn.BackgroundColor = Color.FromHex("#FF778899"); //Gray
                LoginBtn.IsEnabled = false;
            }

            // Changing icons colours to red of invalid entries:
            if(!emailValid && !string.IsNullOrEmpty(EmailEntry.Text))
            {
                invalidEmailIcon = EntryInvalid(EmailEntry, invalidEmailIcon, 1);
            } else if(invalidEmailIcon != null) { invalidEmailIcon = EntryInvalidReset(EmailEntry, invalidEmailIcon); }

            if (!passwordValid && !string.IsNullOrEmpty(PasswordEntry.Text)) { invalidPasswordIcon = EntryInvalid(PasswordEntry, invalidPasswordIcon, 2); }
            else if (invalidPasswordIcon != null) { invalidPasswordIcon = EntryInvalidReset(PasswordEntry, invalidPasswordIcon); }
        }


        // Focused- When highlighted, the icon changes colour:
        private void EmailFocused(object sender, FocusEventArgs e)
        {
            Label EmailIcon = (Label)Content.FindByName("EmailIcon");
            Color newColor = (Color)App.Current.Resources["Primary"]; // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (EmailIcon.TextColor == newColor) { EmailIcon.TextColor = defaultColor; }
            else { EmailIcon.TextColor = newColor; } 
        }
        private void PasswordFocused(object sender, FocusEventArgs e)
        {
            Label PasswordIcon = (Label)Content.FindByName("PasswordIcon");
            Color newColor = (Color)App.Current.Resources["Primary"]; // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (PasswordIcon.TextColor == newColor) { PasswordIcon.TextColor = defaultColor; }
            else { PasswordIcon.TextColor = newColor; }
        }

    }
}