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
        bool running = false;
        public LoadingPage()
        {
            InitializeComponent();
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Encrypted Messaging App/encrypted-messaging-app-b689c5915859.json");
        }

        protected override async void OnAppearing() //object sender, EventArgs e
        {
            if (running) { return; } else { running = true; }
            Console.WriteLine("~~ Loading Page ~~");
            base.OnAppearing();


            // Get messages from firestore
            IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();
            Task<(bool success, object result)> TResponse = FirestoreService.FetchData("CUser");

            var response = await TResponse;
            if (response.success)
            {
                Console.WriteLine("Recieved Result");
                
                CurrentUser = (CUser)response.result;
                

                Console.WriteLine("Recieved user");
                CurrentUser.Output();
                

                // Load main screen:
                await Shell.Current.GoToAsync($"//{nameof(MainMessagePage)}");

                DependencyService.Get<IToastMessage>().LongAlert("Successfully Signed in!!");

            } else if (response.result == "No data found")
            {
                DependencyService.Get<IToastMessage>().LongAlert("Account not found.");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            else
            {
                Console.WriteLine($"Unable to get CUser data: {response.result}");
                DependencyService.Get<IToastMessage>().LongAlert("Unable to get user information");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }      
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            running = false;
        }
    }
}