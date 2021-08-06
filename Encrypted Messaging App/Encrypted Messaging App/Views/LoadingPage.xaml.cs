using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.Views.GlobalVariables;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            InitializeComponent();
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Encrypted Messaging App/encrypted-messaging-app-b689c5915859.json");
            Prepare();
        }

        private async void Prepare() //object sender, EventArgs e
        {
            // Get messages from firestore
            IManageUserService UserService = DependencyService.Resolve<IManageUserService>();
            Task<CUser> Tuser = UserService.GetUser();
            Console.WriteLine("Recieved Result");
            CurrentUser = await Tuser;
 

            Console.WriteLine("Recieved user");
            CurrentUser.Output();

            Console.WriteLine("Going to main screen");
            // Load main screen:
            await Shell.Current.GoToAsync($"//{nameof(MainMessagePage)}");
        }

    }
}