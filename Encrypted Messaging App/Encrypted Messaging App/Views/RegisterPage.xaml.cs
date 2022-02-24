using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.Views.Functions;

//using Encrypted_Messaging_App.Droid;


namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        Label invalidEmailIcon = new Label();
        Label invalidPasswordIcon = new Label();
        Label invalidPassword2Icon = new Label();

        public RegisterPage()
        {
            Console.WriteLine("~~ Register Page ~~");
            InitializeComponent();
        }

        private async void LoginText_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }

        private void ChangeVisibility(object sender, EventArgs e)
        {
            Console.WriteLine("Clicked");
            Label lblClicked = (Label)sender;
            Entry passwordEntry = (Entry)Content.FindByName("PasswordEntry");
            Entry password2Entry = (Entry)Content.FindByName("PasswordConfirmEntry");
            if (lblClicked.Text == "&#xf070;" || lblClicked.Text == "\uf070")
            {
                lblClicked.Text = "\uf06e ";
                Console.WriteLine("Changed to shown");
                passwordEntry.IsPassword = false;
                password2Entry.IsPassword = false;
            }
            else
            {
                lblClicked.Text = "\uf070";
                Console.WriteLine($"Changed to hidden");
                passwordEntry.IsPassword = true;
                password2Entry.IsPassword = true;
            }
        }

        private void EditedEntry(object sender, EventArgs e)
        {
            Button RegisterBtn = (Button)Content.FindByName("RegisterButton");
            Entry UsernameEntry = (Entry)Content.FindByName("UsernameEntry");
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            Entry Password2Entry = (Entry)Content.FindByName("PasswordConfirmEntry");
            String[] texts = new string[] { UsernameEntry.Text, EmailEntry.Text, PasswordEntry.Text, Password2Entry.Text };
            bool filled = true;
            foreach(string text in texts)
            {
                if (String.IsNullOrEmpty(text)) { filled = false; }
                else if (text.Length == 0) { filled = false; }
            }
            bool emailValid = isValidEmail(EmailEntry.Text);
            bool passwordValid = PasswordEntry.Text == null || PasswordEntry.Text.Length >= 6;
            bool password2Valid = PasswordEntry.Text == null || Password2Entry.Text == null || Password2Entry.Text == PasswordEntry.Text;
            

            if (filled && emailValid && passwordValid && password2Valid)
            {
                RegisterBtn.BackgroundColor = Color.FromHex("#2196F3"); //Primary
                RegisterBtn.IsEnabled = true;
            }
            else
            {
                RegisterBtn.IsEnabled = false;
                RegisterBtn.BackgroundColor = Color.FromHex("#FF778899"); //Gray
            }
            if (!emailValid && !String.IsNullOrEmpty(EmailEntry.Text))
            {
                invalidEmailIcon = EntryInvalid(EmailEntry, invalidEmailIcon, 2);
            } else if (invalidEmailIcon != null)
            {
                invalidEmailIcon = EntryInvalidReset(EmailEntry, invalidEmailIcon);
            }
            if (!passwordValid) { invalidPasswordIcon = EntryInvalid(PasswordEntry, invalidPasswordIcon, 3); }
            else if(invalidPasswordIcon != null) { invalidPasswordIcon = EntryInvalidReset(EmailEntry, invalidPasswordIcon); }

            if (!password2Valid) { invalidPassword2Icon = EntryInvalid(Password2Entry, invalidPassword2Icon, 4); }
            else if (invalidPassword2Icon != null) { invalidPassword2Icon = EntryInvalidReset(EmailEntry, invalidPassword2Icon); }
        }

        private void EntryFocused(string name)
        {
            Label Icon = (Label)Content.FindByName($"{name}Icon");
            IconInvalidReset(Icon);
            Color newColor = (Color)App.Current.Resources["Primary"]; // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (Icon.TextColor == newColor) { Icon.TextColor = defaultColor; }
            else { Icon.TextColor = newColor; }
        }
        private void PasswordFocused(object sender, FocusEventArgs e)
        {
            EntryFocused("Password");
        }
        private void UsernameFocused(object sender, FocusEventArgs e)
        {
            EntryFocused("Username");
        }
        private void EmailFocused(object sender, FocusEventArgs e)
        {
            EntryFocused("Email");
        }

        private Button RegisterBtn;


        private async void RegisterButton_Clicked(object sender, EventArgs e)
        {
            Entry UsernameEntry = (Entry)Content.FindByName("UsernameEntry");
            Entry EmailEntry = (Entry)Content.FindByName("EmailEntry");
            Entry PasswordEntry = (Entry)Content.FindByName("PasswordEntry");
            RegisterBtn = (Button)Content.FindByName("RegisterButton");
            string username = UsernameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;


            IAuthenticationService AuthenticationService = DependencyService.Resolve<IAuthenticationService>();
            if(AuthenticationService == null) {
                Console.WriteLine("Not resolved");
                return;
            }
            var resultObj = await AuthenticationService.Register(username, email, password);
            bool result = resultObj.Item1;

            if (!result)
            {
                string type = resultObj.Item2;
                if (type != "")
                {
                    Label InvalidIcon = (Label)Content.FindByName($"{type}Icon");
                    IconInvalid(InvalidIcon);
                }

                RegisterBtn.IsEnabled = false;
                RegisterBtn.BackgroundColor = (Color)App.Current.Resources["Invalid"];

            }
            else
            {
                IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();
                (bool success, string message) registerResult = await FirestoreService.InitiliseUser(username);
                if (registerResult.success)
                {
                    await Shell.Current.GoToAsync($"//{nameof(LoadingPage)}");
                }
                else
                {
                    IToastMessage toastMessage = DependencyService.Resolve<IToastMessage>();
                    toastMessage.LongAlert($"Unable to register: {registerResult.message}");
                    Console.WriteLine($"TOAST MESSAGE- Unable to register: {registerResult.message}");
                }
                
            }


        }
        private async void Login_Tap(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        //private void PasswordConfirmFocused(object sender, FocusEventArgs e)
        //{
        //    EntryFocused("PasswordConfirm");
        //}
    }
}