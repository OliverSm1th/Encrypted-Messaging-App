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
    {   bool running = false;
        public LoadingPage()  { InitializeComponent(); }

        protected override async void OnAppearing()
        {
            if (running) { return; } else { running = true; }
            Console.WriteLine("~~ Loading Page ~~");
            base.OnAppearing();

            // Get messages from firestore
            IManageFirestoreService FirestoreService = DependencyService.Resolve<IManageFirestoreService>();
            (bool success, object result) response = await FirestoreService.FetchData<CUser>("CUser");
            if (response.success)
            {               
                CurrentUser = (CUser)response.result;
                CurrentUser.Output();
                
                // Load main screen:
                await Shell.Current.GoToAsync($"//{nameof(MainMessagePage)}");
                DependencyService.Get<IToastMessage>().LongAlert("Successfully Signed in!!");
            } else if (response.result == "No data found")
            {   DependencyService.Get<IToastMessage>().LongAlert("Account not found.");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            else
            {   Console.WriteLine($"Unable to get CUser data: {response.result}");
                DependencyService.Get<IToastMessage>().LongAlert("Unable to get user information");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }      
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            running = false;
        }
    }
}