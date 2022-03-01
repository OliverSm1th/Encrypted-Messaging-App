using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Xamarin.Forms;
using Android.Gms.Extensions;
using Encrypted_Messaging_App.Droid;




[assembly: Dependency(typeof(FirebaseAuthentication))]
namespace Encrypted_Messaging_App.Droid
{
    public class FirebaseAuthentication : IAuthenticationService
    {
        public bool isSignedIn()
        {
            FirebaseUser currentUser = FirebaseAuth.Instance.CurrentUser;
            return currentUser != null;
        }

        public async Task<(bool, string)> LogIn(string email, string password)
        {
            try
            {
                IAuthResult Result = await FirebaseAuth.Instance.SignInWithEmailAndPasswordAsync(email, password);
                var Token = await Result.User.GetIdToken(false).AsAsync<GetTokenResult>();

                Console.WriteLine($"Recieved Token: {Token.Token}");
                return (true, Token.Token);

            }
            catch(Exception e)
            {
                string target = "";
                string[] emailErrors = new string[] { "The email address is badly formatted.", "There is no user record corresponding to this identifier. The user may have been deleted.", "There is no user record corresponding to this identifier. The user may have been deleted." };
                string[] passwordErrors = new string[] { "The password is invalid or the user does not have a password." };

                if(passwordErrors.Contains(e.Message)) { target = "Password"; }
                else if (emailErrors.Contains(e.Message)) { target = "Email"; }

                Console.WriteLine($"LogIn Error: {e.Message}");
                return (false, target);
            }
        }

        public bool LogOut()
        {
            try
            {
                FirebaseAuth.Instance.SignOut();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine($"LogOut Error: {e.Message}");
                return false;
            }
        }

        public async Task<(bool, string)> Register(string username, string email, string password)
        {
            try
            {
                IAuthResult Result = await Firebase.Auth.FirebaseAuth.Instance.CreateUserWithEmailAndPasswordAsync(email, password);
                UserProfileChangeRequest changeRequest = new UserProfileChangeRequest.Builder().SetDisplayName(username).Build();
                await Result.User.UpdateProfileAsync(changeRequest);
                return (true, "");

            }
            catch (Exception e)
            {
                string[] emailErrors = new string[] { "The email address is badly formatted.", "The email address is already in use by another account."};
                Console.WriteLine($"SignUp Error: {e.Message}");
                string target = "";
                if(emailErrors.Contains(e.Message)) { target = "Email"; }

                //DependencyService.Get<IMessage>().ShortAlert(e.Message);
                return (false, target);
            }
        }

        public async Task<bool> ForgotPassword(string email)
        {
            try
            {
                await FirebaseAuth.Instance.SendPasswordResetEmailAsync(email);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine($"ForgotPassword Error: {e.Message}");
                return false;
            }
        }

    }
}